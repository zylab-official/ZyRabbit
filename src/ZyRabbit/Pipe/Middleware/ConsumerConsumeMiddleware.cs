using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using ZyRabbit.Configuration.Consume;
using ZyRabbit.Consumer;

namespace ZyRabbit.Pipe.Middleware
{
	public class BasicConsumeOptions
	{
		public Func<IPipeContext, ConsumeConfiguration> ConsumeConfigFunc { get; set; }
		public Func<IPipeContext, IBasicConsumer> ConsumerFunc { get; set; }
		public Func<IPipeContext, bool> ConfigValidatePredicate { get; set; }
	}

	public class ConsumerConsumeMiddleware : Middleware
	{
		private readonly IConsumerFactory _factory;
		protected Func<IPipeContext, ConsumeConfiguration> ConsumeConfigFunc;
		protected Func<IPipeContext, IBasicConsumer> ConsumerFunc;
		protected Func<IPipeContext, bool> ConfigValidatePredicate;
		private readonly ILogger<ConsumerConsumeMiddleware> Logger;

		public ConsumerConsumeMiddleware(IConsumerFactory factory, ILogger<ConsumerConsumeMiddleware> logger, BasicConsumeOptions options = null)
		{
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));
			Logger = logger  ?? throw new ArgumentNullException(nameof(logger));
			ConsumeConfigFunc = options?.ConsumeConfigFunc ?? (context => context.GetConsumeConfiguration());
			ConsumerFunc = options?.ConsumerFunc ?? (context => context.GetConsumer());
			ConfigValidatePredicate = options?.ConfigValidatePredicate ?? (context => true);
		}

		public override async Task InvokeAsync(IPipeContext context, CancellationToken token = new CancellationToken())
		{
			var config = GetConsumeConfiguration(context);
			if (config == null)
			{
				Logger.LogWarning("Consumer configuration not found, skipping consume.");
				return;
			}

			var consumer = GetConsumer(context);
			if (consumer == null)
			{
				Logger.LogWarning("Consumer not found. Will not consume on queue {queueName}.", config.QueueName);
				return;
			}

			BasicConsume(consumer, config);
			await Next.InvokeAsync(context, token);
		}

		protected virtual ConsumeConfiguration GetConsumeConfiguration(IPipeContext context)
		{
			return ConsumeConfigFunc?.Invoke(context);
		}

		protected virtual IBasicConsumer GetConsumer(IPipeContext context)
		{
			return ConsumerFunc?.Invoke(context);
		}

		protected virtual void BasicConsume(IBasicConsumer consumer, ConsumeConfiguration config)
		{
			_factory.ConfigureConsume(consumer, config);
		}
	}
}
