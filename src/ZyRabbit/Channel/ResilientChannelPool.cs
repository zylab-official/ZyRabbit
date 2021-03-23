using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using ZyRabbit.Channel.Abstraction;

namespace ZyRabbit.Channel
{
	public class ResilientChannelPool : DynamicChannelPool
	{
		protected readonly IChannelFactory ChannelFactory;
		private readonly int _desiredChannelCount;

		public ResilientChannelPool(IChannelFactory factory, ILogger<IChannelPool> logger, int channelCount)
			: this(factory, logger, CreateSeed(factory, channelCount)) { }

		public ResilientChannelPool(IChannelFactory factory, ILogger<IChannelPool> logger)
			: this(factory, logger, Enumerable.Empty<IModel>()) { }

		public ResilientChannelPool(IChannelFactory factory, ILogger<IChannelPool> logger, IEnumerable<IModel> seed) : base(seed, logger)
		{
			ChannelFactory = factory ?? throw new ArgumentNullException(nameof(factory));
			_desiredChannelCount = (seed ?? throw new ArgumentNullException(nameof(seed))).Count();
		}

		private static IEnumerable<IModel> CreateSeed(IChannelFactory factory, int channelCount)
		{
			for (var i = 0; i < channelCount; i++)
			{
				yield return factory.CreateChannelAsync().GetAwaiter().GetResult();
			}
		}

		public override async Task<IModel> GetAsync(CancellationToken ct = default(CancellationToken))
		{
			var currentCount = GetActiveChannelCount();
			if (currentCount < _desiredChannelCount)
			{
				var createCount = _desiredChannelCount - currentCount;
				for (var i = 0; i < createCount; i++)
				{
					var channel = await ChannelFactory.CreateChannelAsync(ct);
					Add(channel);
				}
			}
			return await base.GetAsync(ct);
		}
	}
}
