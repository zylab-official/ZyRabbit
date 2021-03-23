using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Common;

namespace ZyRabbit.Pipe.Middleware
{
	public class QueueBindOptions
	{
		public Func<IPipeContext, string> QueueNameFunc { get; set; }
		public Func<IPipeContext, string> ExchangeNameFunc { get; set; }
		public Func<IPipeContext, string> RoutingKeyFunc { get; set; }
	}

	public class QueueBindMiddleware : Middleware
	{
		protected readonly ITopologyProvider TopologyProvider;
		protected readonly Func<IPipeContext, string> QueueNameFunc;
		protected readonly Func<IPipeContext, string> ExchangeNameFunc;
		protected readonly Func<IPipeContext, string> RoutingKeyFunc;
		protected readonly ILogger<QueueBindMiddleware> Logger;

		public QueueBindMiddleware(ITopologyProvider topologyProvider, ILogger<QueueBindMiddleware> logger, QueueBindOptions options = null)
		{
			TopologyProvider = topologyProvider ?? throw new ArgumentNullException(nameof(topologyProvider));
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			QueueNameFunc = options?.QueueNameFunc ?? (context => context.GetConsumeConfiguration()?.QueueName);
			ExchangeNameFunc = options?.ExchangeNameFunc ?? (context => context.GetConsumeConfiguration()?.ExchangeName);
			RoutingKeyFunc = options?.RoutingKeyFunc ?? (context => context.GetConsumeConfiguration()?.RoutingKey);
		}

		public override async Task InvokeAsync(IPipeContext context, CancellationToken token)
		{
			var queueName = GetQueueName(context);
			var exchangeName = GetExchangeName(context);
			var routingKey = GetRoutingKey(context);

			await BindQueueAsync(queueName, exchangeName, routingKey, context, token);
			await Next.InvokeAsync(context, token);
		}

		protected virtual Task BindQueueAsync(string queue, string exchange, string routingKey, IPipeContext context, CancellationToken ct)
		{
			return TopologyProvider.BindQueueAsync(queue, exchange, routingKey, context.GetConsumeConfiguration()?.Arguments);
		}

		protected virtual string GetRoutingKey(IPipeContext context)
		{
			var routingKey = RoutingKeyFunc(context);
			if (routingKey == null)
			{
				Logger.LogWarning("Routing key not found in Pipe context.");
			}
			return routingKey;
		}

		protected virtual string GetExchangeName(IPipeContext context)
		{
			var exchange = ExchangeNameFunc(context);
			if (exchange == null)
			{
				Logger.LogWarning("Exchange name not found in Pipe context.");
			}
			return exchange;
		}

		protected virtual string GetQueueName(IPipeContext context)
		{
			var queue = QueueNameFunc(context);
			if (queue == null)
			{
				Logger.LogWarning("Queue name not found in Pipe context.");
			}
			return queue;
		}
	}
}
