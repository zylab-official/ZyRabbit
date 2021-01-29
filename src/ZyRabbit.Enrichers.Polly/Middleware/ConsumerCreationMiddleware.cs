using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using ZyRabbit.Consumer;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit.Enrichers.Polly.Middleware
{
	public class ConsumerCreationMiddleware : Pipe.Middleware.ConsumerCreationMiddleware
	{
		public ConsumerCreationMiddleware(IConsumerFactory consumerFactory, ILogger<Pipe.Middleware.ConsumerCreationMiddleware> logger, ConsumerCreationOptions options = null)
			: base(consumerFactory, logger, options) { }

		protected override Task<IBasicConsumer> GetOrCreateConsumerAsync(IPipeContext context, CancellationToken token)
		{
			var policy = context.GetPolicy(PolicyKeys.QueueDeclare);
			return policy.ExecuteAsync(
				action: ct => base.GetOrCreateConsumerAsync(context, ct),
				cancellationToken: token,
				contextData: new Dictionary<string, object>
				{
					[RetryKey.PipeContext] = context,
					[RetryKey.CancellationToken] = token,
					[RetryKey.ConsumerFactory] = ConsumerFactory,
				}
			);
		}
	}
}
