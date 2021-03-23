using System.Collections.Generic;
using RabbitMQ.Client;
using ZyRabbit.Common;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ZyRabbit.Enrichers.Polly.Middleware
{
	public class BasicPublishMiddleware : Pipe.Middleware.BasicPublishMiddleware
	{
		public BasicPublishMiddleware(IExclusiveLock exclusive, ILogger<Pipe.Middleware.BasicPublishMiddleware> logger, BasicPublishOptions options = null)
			: base(exclusive, logger, options) { }

		protected override void BasicPublish(
				IModel channel,
				string exchange,
				string routingKey,
				bool mandatory,
				IBasicProperties basicProps,
				byte[] body,
				IPipeContext context)
		{
			var policy = context.GetPolicy(PolicyKeys.BasicPublish);
			var policyTask = policy.ExecuteAsync(
				action: () =>
				{
					base.BasicPublish(channel, exchange, routingKey, mandatory, basicProps, body, context);
					return Task.FromResult(true);
				},
				contextData: new Dictionary<string, object>
				{
					[RetryKey.PipeContext] = context,
					[RetryKey.ExchangeName] = exchange,
					[RetryKey.RoutingKey] = routingKey,
					[RetryKey.PublishMandatory] = mandatory,
					[RetryKey.BasicProperties] = basicProps,
					[RetryKey.PublishBody] = body,
				});
			policyTask.ConfigureAwait(false);
			policyTask.GetAwaiter().GetResult();
		}
	}
}
