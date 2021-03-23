using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Configuration.Consume;

namespace ZyRabbit.Pipe.Middleware
{
	public class ConsumeConfigurationOptions
	{
		public Func<IPipeContext, string> QueueFunc { get; set; }
		public Func<IPipeContext, string> RoutingKeyFunc { get; set; }
		public Func<IPipeContext, string> ExchangeFunc { get; set; }
		public Func<IPipeContext, Type> MessageTypeFunc { get; set; }
		public Func<IPipeContext, Action<IConsumeConfigurationBuilder>> ConfigActionFunc { get; set; }
	}

	public class ConsumeConfigurationMiddleware : Middleware
	{
		protected readonly IConsumeConfigurationFactory ConfigFactory;
		protected Func<IPipeContext, string> QueueFunc;
		protected Func<IPipeContext, string> ExchangeFunc;
		protected Func<IPipeContext, string> RoutingKeyFunc;
		protected Func<IPipeContext, Type> MessageTypeFunc;
		protected Func<IPipeContext, Action<IConsumeConfigurationBuilder>> ConfigActionFunc;
		protected readonly ILogger<ConsumeConfigurationMiddleware> Logger;

		public ConsumeConfigurationMiddleware(IConsumeConfigurationFactory configFactory, ILogger<ConsumeConfigurationMiddleware> logger, ConsumeConfigurationOptions options = null)
		{
			ConfigFactory = configFactory ?? throw new ArgumentNullException(nameof(configFactory));
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			QueueFunc = options?.QueueFunc ?? (context => context.GetQueueDeclaration()?.Name);
			ExchangeFunc = options?.ExchangeFunc ?? (context => context.GetExchangeDeclaration()?.Name);
			RoutingKeyFunc = options?.RoutingKeyFunc ?? (context => context.GetRoutingKey());
			MessageTypeFunc = options?.MessageTypeFunc ?? (context => context.GetMessageType());
			ConfigActionFunc = options?.ConfigActionFunc ?? (context => context.Get<Action<IConsumeConfigurationBuilder>>(PipeKey.ConfigurationAction));
		}

		public override async Task InvokeAsync(IPipeContext context, CancellationToken token)
		{
			var config = ExtractConfigFromMessageType(context) ?? ExtractConfigFromStrings(context);

			var action = GetConfigurationAction(context);
			if (action != null)
			{
				Logger.LogInformation("Configuration action for {queueName} found.", config?.QueueName);
				var builder = new ConsumeConfigurationBuilder(config);
				action(builder);
				config = builder.Config;
			}

			context.Properties.TryAdd(PipeKey.ConsumeConfiguration, config);

			await Next.InvokeAsync(context, token);
		}

		protected virtual Type GetMessageType(IPipeContext context)
		{
			return MessageTypeFunc(context);
		}

		protected Action<IConsumeConfigurationBuilder> GetConfigurationAction(IPipeContext context)
		{
			return ConfigActionFunc(context);
		}

		protected virtual ConsumeConfiguration ExtractConfigFromStrings(IPipeContext context)
		{
			var routingKey = RoutingKeyFunc(context);
			var queueName = QueueFunc(context);
			var exchangeName = ExchangeFunc(context);
			Logger.LogDebug("Consuming from queue {queueName} on {exchangeName} with routing key {routingKey}", queueName, exchangeName, routingKey);
			return ConfigFactory.Create(queueName, exchangeName, routingKey);
		}

		protected virtual ConsumeConfiguration ExtractConfigFromMessageType(IPipeContext context)
		{
			var messageType = MessageTypeFunc(context);
			if (messageType != null)
			{
				Logger.LogDebug("Found message type {messageType} in context. Creating consume config based on it.", messageType.Name);
			}
			return messageType == null
				? null
				: ConfigFactory.Create(messageType);
		}
	}

	public static class BasicConsumeExtensions
	{
		public static TPipeContext UseConsumeConfiguration<TPipeContext>(this TPipeContext context, Action<IConsumeConfigurationBuilder> config) where TPipeContext : IPipeContext
		{
			context.Properties.TryAdd(PipeKey.ConfigurationAction, config);
			return context;
		}
	}
}
