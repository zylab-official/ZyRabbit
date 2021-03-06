﻿using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Common;
using ZyRabbit.Configuration.Exchange;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit.Enrichers.Polly.Middleware
{
	public class ExchangeDeclareMiddleware : Pipe.Middleware.ExchangeDeclareMiddleware
	{
		public ExchangeDeclareMiddleware(ITopologyProvider topologyProvider, ILogger<Pipe.Middleware.ExchangeDeclareMiddleware> logger, ExchangeDeclareOptions options = null)
			: base(topologyProvider, logger, options)
		{
		}

		protected override Task DeclareExchangeAsync(ExchangeDeclaration exchange, IPipeContext context, CancellationToken token)
		{
			var policy = context.GetPolicy(PolicyKeys.ExchangeDeclare);
			return policy.ExecuteAsync(
				action: ct => base.DeclareExchangeAsync(exchange, context, ct),
				cancellationToken: token,
				contextData: new Dictionary<string, object>
				{
					[RetryKey.TopologyProvider] = TopologyProvider,
					[RetryKey.ExchangeDeclaration] = exchange,
					[RetryKey.PipeContext] = context,
					[RetryKey.CancellationToken] = token,
				});
		}
	}
}
