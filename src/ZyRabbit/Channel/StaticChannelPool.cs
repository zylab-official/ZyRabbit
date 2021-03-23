using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using ZyRabbit.Exceptions;

namespace ZyRabbit.Channel
{
	public interface IChannelPool
	{
		Task<IModel> GetAsync(CancellationToken ct = default(CancellationToken));
	}

	public class StaticChannelPool : IDisposable, IChannelPool
	{
		protected readonly LinkedList<IModel> Pool;
		protected readonly List<IRecoverable> Recoverables;
		protected readonly ConcurrentChannelQueue ChannelRequestQueue;
		private readonly object _workLock = new object();
		protected readonly ILogger<IChannelPool> Logger;
		private LinkedListNode<IModel> _current;

		public StaticChannelPool(IEnumerable<IModel> seed, ILogger<IChannelPool> logger)
		{
			seed = (seed ?? throw new ArgumentNullException(nameof(seed))).ToList();
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			Pool = new LinkedList<IModel>(seed);
			Recoverables = new List<IRecoverable>();
			ChannelRequestQueue = new ConcurrentChannelQueue();
			ChannelRequestQueue.Queued += (sender, args) => StartServeChannels();
			foreach (var channel in seed)
			{
				ConfigureRecovery(channel);
			}
		}

		private void StartServeChannels()
		{
			if (ChannelRequestQueue.IsEmpty || Pool.Count == 0)
			{
				Logger.LogInformation("Unable to serve channels. The pool consists of {channelCount} channels and {channelRequests} requests for channels.");
				return;
			}

			if (!Monitor.TryEnter(_workLock))
			{
				Logger.LogDebug("Unable to acquire work lock for service channels.");
				return;
			}

			try
			{
				Logger.LogDebug("Starting serving channels.");
				do
				{
					_current = _current?.Next ?? Pool.First;
					if (_current == null)
					{
						Logger.LogDebug("Unable to server channels. Pool empty.");
						return;
					}

					if (_current.Value.IsClosed)
					{
						Pool.Remove(_current);
						if (Pool.Count != 0)
						{
							continue;
						}

						if (Recoverables.Count == 0)
						{
							throw new ChannelAvailabilityException("No open channels in pool and no recoverable channels");
						}

						Logger.LogInformation("No open channels in pool, but {recoveryCount} waiting for recovery", Recoverables.Count);
						return;
					}

					if (ChannelRequestQueue.TryDequeue(out var cTsc))
					{
						cTsc.TrySetResult(_current.Value);
					}
				} while (!ChannelRequestQueue.IsEmpty);
			}
			catch (ChannelAvailabilityException e)
			{
				Logger.LogError(e.Message);
				throw;
			}
			catch (Exception e)
			{
				Logger.LogError(e, "An unhandled exception occured when serving channels.");
			}
			finally
			{
				Monitor.Exit(_workLock);
			}
		}

		protected virtual int GetActiveChannelCount()
		{
			return Enumerable
				.Concat<object>(Pool, Recoverables)
				.Distinct()
				.Count();
		}

		protected void ConfigureRecovery(IModel channel)
		{
			if (!(channel is IRecoverable recoverable))
			{
				Logger.LogWarning("Channel {channelNumber} is not recoverable. Recovery disabled for this channel.", channel.ChannelNumber);
				return;
			}
			if (channel.IsClosed && channel.CloseReason != null && channel.CloseReason.Initiator == ShutdownInitiator.Application)
			{
				Logger.LogWarning("{Channel {channelNumber} is closed by the application. Channel will remain closed and not be part of the channel pool", channel.ChannelNumber);
				return;
			}
			Recoverables.Add(recoverable);
			recoverable.Recovery += (sender, args) =>
			{
				Logger.LogInformation("Channel {channelNumber} has been recovered and will be re-added to the channel pool", channel.ChannelNumber);
				if (Pool.Contains(channel))
				{
					return;
				}
				Pool.AddLast(channel);
				StartServeChannels();
			};
			channel.ModelShutdown += (sender, args) =>
			{
				if (args.Initiator == ShutdownInitiator.Application)
				{
					Logger.LogWarning("Channel {channelNumber} is being closed by the application. No recovery will be performed.", channel.ChannelNumber);
					Recoverables.Remove(recoverable);
				}
			};
		}

		public virtual Task<IModel> GetAsync(CancellationToken ct = default(CancellationToken))
		{
			var channelTcs = ChannelRequestQueue.Enqueue();
			ct.Register(() => channelTcs.TrySetCanceled());
			return channelTcs.Task;
		}

		public virtual void Dispose()
		{
			foreach (var channel in Pool)
			{
				channel?.Dispose();
			}
			foreach (var recoverable in Recoverables)
			{
				(recoverable as IModel)?.Dispose();
			}
		}
	}
}
