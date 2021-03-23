using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace ZyRabbit.Channel
{
	public class DynamicChannelPool : StaticChannelPool
	{
		public DynamicChannelPool(ILogger<IChannelPool> logger)
			: this(Enumerable.Empty<IModel>(), logger) { }

		public DynamicChannelPool(IEnumerable<IModel> seed, ILogger<IChannelPool> logger)
			: base(seed, logger) { }

		public void Add(params IModel[] channels)
		{
			Add(channels.ToList());
		}

		public void Add(IEnumerable<IModel> channels)
		{
			foreach (var channel in channels)
			{
				ConfigureRecovery(channel);
				if (Pool.Contains(channel))
				{
					continue;
				}
				Pool.AddLast(channel);
			}
		}

		public void Remove(int numberOfChannels = 1)
		{
			var toRemove = Pool
				.Take(numberOfChannels)
				.ToList();
			Remove(toRemove);
		}

		public void Remove(params IModel[] channels)
		{
			Remove(channels.ToList());
		}

		public void Remove(IEnumerable<IModel> channels)
		{
			foreach (var channel in channels)
			{
				Pool.Remove(channel);
				Recoverables.Remove(channel as IRecoverable);
			}
		}
	}
}
