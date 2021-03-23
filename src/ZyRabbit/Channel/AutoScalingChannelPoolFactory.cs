using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using ZyRabbit.Channel.Abstraction;

namespace ZyRabbit.Channel
{
	public interface IChannelPoolFactory
	{
		IChannelPool GetChannelPool(string name = null);
	}

	public class AutoScalingChannelPoolFactory : IChannelPoolFactory, IDisposable
	{
		private readonly IChannelFactory _factory;
		private readonly ILogger<IChannelPool> _poolLogger;
		private readonly AutoScalingOptions _options;
		private readonly ConcurrentDictionary<string, Lazy<IChannelPool>> _channelPools;
		private const string DefaultPoolName = "default";

		public AutoScalingChannelPoolFactory(IChannelFactory factory, ILogger<IChannelPool> poolLogger, AutoScalingOptions options = null)
		{
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));
			_poolLogger = poolLogger ?? throw new ArgumentNullException(nameof(poolLogger));
			_options = options ?? AutoScalingOptions.Default;
			_channelPools = new ConcurrentDictionary<string, Lazy<IChannelPool>>();
		}

		public IChannelPool GetChannelPool(string name = null)
		{
			name = name ?? DefaultPoolName;
			var pool = _channelPools.GetOrAdd(name, s => new Lazy<IChannelPool>(() => new AutoScalingChannelPool(_factory, _poolLogger, _options)));
			return pool.Value;
		}

		public void Dispose()
		{
			_factory?.Dispose();
			foreach (var channelPool in _channelPools.Values)
			{
				(channelPool.Value as IDisposable)?.Dispose();
			}
		}
	}
}
