using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using ZyRabbit.Consumer;

namespace ZyRabbit.Pipe.Middleware
{
	public class ConsumerCreationOptions
	{
		public Func<IConsumerFactory, CancellationToken, IPipeContext, Task<IBasicConsumer>> ConsumerFunc { get; set; }
	}

	public class ConsumerCreationMiddleware : Middleware
	{
		protected readonly IConsumerFactory ConsumerFactory;
		protected readonly Func<IConsumerFactory, CancellationToken, IPipeContext, Task<IBasicConsumer>> ConsumerFunc;
		protected readonly ILogger<ConsumerCreationMiddleware> Logger;

		public ConsumerCreationMiddleware(IConsumerFactory consumerFactory, ILogger<ConsumerCreationMiddleware> logger, ConsumerCreationOptions options = null)
		{
			ConsumerFactory = consumerFactory ?? throw new ArgumentNullException(nameof(consumerFactory));
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			ConsumerFunc = options?.ConsumerFunc ?? ((factory, token, context) => factory.CreateConsumerAsync(context.GetChannel(), token));
		}

		public override async Task InvokeAsync(IPipeContext context, CancellationToken token = default(CancellationToken))
		{
			var consumer = await GetOrCreateConsumerAsync(context, token);
			context.Properties.TryAdd(PipeKey.Consumer, consumer);
			await Next.InvokeAsync(context, token);
		}

		protected virtual Task<IBasicConsumer> GetOrCreateConsumerAsync(IPipeContext context, CancellationToken token)
		{
			var consumerTask = ConsumerFunc(ConsumerFactory, token, context);
			if (consumerTask == null)
			{
				Logger.LogWarning("No Consumer creation task found in Pipe context.");
			}
			return consumerTask;
		}
	}
}
