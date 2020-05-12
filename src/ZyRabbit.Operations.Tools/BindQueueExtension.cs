using System;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Configuration.Consume;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit
{
	public static class BindQueueExtension
	{
		public static readonly Action<IPipeBuilder> BindQueueAction = pipe => pipe
			.Use<ConsumeConfigurationMiddleware>()
			.Use<QueueBindMiddleware>();

		public static Task BindQueueAsync(this IBusClient client, string queueName, string exchangeName, string routingKey, CancellationToken ct = default (CancellationToken))
		{
			return client.InvokeAsync(BindQueueAction, cfg =>
			{
				cfg.Properties.AddOrReplace(PipeKey.ConsumeConfiguration, new ConsumeConfiguration
				{
					QueueName = queueName,
					ExchangeName = exchangeName,
					RoutingKey = routingKey
				});
			}, ct);
		}

		public static Task BindQueueAsync<TMessage>(this IBusClient client, CancellationToken ct = default(CancellationToken))
		{
			return client.InvokeAsync(BindQueueAction, cfg =>
			{
				cfg.Properties.AddOrReplace(PipeKey.MessageType, typeof(TMessage));
			}, ct);
		}
	}
}
