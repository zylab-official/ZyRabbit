﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using ZyRabbit.Channel.Abstraction;
using ZyRabbit.Pipe;

namespace ZyRabbit.Enrichers.Polly.Middleware
{
	public class TransientChannelMiddleware : Pipe.Middleware.TransientChannelMiddleware
	{
		public TransientChannelMiddleware(IChannelFactory factory, ILogger<Pipe.Middleware.TransientChannelMiddleware> logger)
			: base(factory, logger) { }

		protected override Task<IModel> CreateChannelAsync(IPipeContext context, CancellationToken token)
		{
			var policy = context.GetPolicy(PolicyKeys.ChannelCreate);
			return policy.ExecuteAsync(
				action: ct => base.CreateChannelAsync(context, ct),
				cancellationToken: token,
				contextData: new Dictionary<string, object>
				{
					[RetryKey.PipeContext] = context,
					[RetryKey.CancellationToken] = token,
					[RetryKey.ChannelFactory] = ChannelFactory
				}
			);
		}
	}
}
