using ZyRabbit.Common;
using ZyRabbit.Enrichers.GlobalExecutionId.Middleware;
using ZyRabbit.Instantiation;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit.Enrichers.GlobalExecutionId
{
	public static class GlobalExecutionIdPlugin
	{
		public static IClientBuilder UseGlobalExecutionId(this IClientBuilder builder)
		{
			builder.Register(pipe => pipe
				// Pulisher
				.Use<AppendGlobalExecutionIdMiddleware>()
				.Use<ExecutionIdRoutingMiddleware>()
				.Use<PublishHeaderAppenderMiddleware>()

				// Subscriber
				.Use<WildcardRoutingKeyMiddleware>()

				// Message Received
				.Use<HeaderDeserializationMiddleware>(new HeaderDeserializationOptions
				{
					HeaderKeyFunc = c => PropertyHeaders.GlobalExecutionId,
					HeaderTypeFunc = c => typeof(string),
					ContextSaveAction = (ctx, id) => ctx.Properties.TryAdd(PipeKey.GlobalExecutionId, id)
				})
				.Use<PersistGlobalExecutionIdMiddleware>()
			);
			return builder;
		}
	}
}
