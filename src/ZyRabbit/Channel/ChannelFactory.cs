using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using ZyRabbit.Channel.Abstraction;
using ZyRabbit.Configuration;
using ZyRabbit.Exceptions;

namespace ZyRabbit.Channel
{
	public class ChannelFactory : IChannelFactory
	{
		protected readonly ILogger<ChannelFactory> Logger;
		protected readonly IConnectionFactory ConnectionFactory;
		protected readonly ZyRabbitConfiguration ClientConfig;
		protected readonly ConcurrentBag<IModel> Channels;
		protected IConnection Connection;

		public ChannelFactory(IConnectionFactory connectionFactory, ZyRabbitConfiguration config, ILogger<ChannelFactory> logger)
		{
			ConnectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
			ClientConfig = config ?? throw new ArgumentNullException(nameof(config));
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			Channels = new ConcurrentBag<IModel>();
		}

		public virtual Task ConnectAsync(CancellationToken token = default(CancellationToken))
		{
			try
			{
				Logger.LogInformation("Creating a new connection for {hostNameCount} hosts.", ClientConfig.Hostnames.Count);
				Connection = ConnectionFactory.CreateConnection(ClientConfig.Hostnames, ClientConfig.ClientProvidedName);
				Connection.ConnectionShutdown += (sender, args) =>
					Logger.LogWarning("Connection was shutdown by {Initiator}. ReplyText {ReplyText}", args.Initiator, args.ReplyText);
			}
			catch (BrokerUnreachableException e)
			{
				Logger.LogError("Unable to connect to broker", e);
				throw;
			}
			return Task.CompletedTask;
		}

		public virtual async Task<IModel> CreateChannelAsync(CancellationToken token = default(CancellationToken))
		{
			var connection = await GetConnectionAsync(token);
			token.ThrowIfCancellationRequested();
			var channel = connection.CreateModel();
			Channels.Add(channel);
			return channel;
		}

		protected virtual async Task<IConnection> GetConnectionAsync(CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			if (Connection == null)
			{
				await ConnectAsync(token);
			}
			if (Connection.IsOpen)
			{
				Logger.LogDebug("Existing connection is open and will be used.");
				return Connection;
			}
			Logger.LogInformation("The existing connection is not open.");

			if (Connection.CloseReason != null &&Connection.CloseReason.Initiator == ShutdownInitiator.Application)
			{
				Logger.LogWarning("Connection is closed with Application as initiator. It will not be recovered.");
				Connection.Dispose();
				throw new ChannelAvailabilityException("Closed connection initiated by the Application. A new connection will not be created, and no channel can be created.");
			}

			if (!(Connection is IRecoverable recoverable))
			{
				Logger.LogWarning("Connection is not recoverable");
				Connection.Dispose();
				throw new ChannelAvailabilityException("The non recoverable connection is closed. A channel can not be created.");
			}

			Logger.LogDebug("Connection is recoverable. Waiting for 'Recovery' event to be triggered. ");
			var recoverTcs = new TaskCompletionSource<IConnection>();
			token.Register(() => recoverTcs.TrySetCanceled());

			EventHandler<EventArgs> completeTask = null;
			completeTask = (sender, args) =>
			{
				if (recoverTcs.Task.IsCanceled)
				{
					return;
				}
				Logger.LogInformation("Connection has been recovered!");
				recoverTcs.TrySetResult(recoverable as IConnection);
				recoverable.Recovery -= completeTask;
			};

			recoverable.Recovery += completeTask;
			return await recoverTcs.Task;
		}

		public void Dispose()
		{
			foreach (var channel in Channels)
			{
				channel?.Dispose();
			}
			Connection?.Dispose();
		}
	}
}
