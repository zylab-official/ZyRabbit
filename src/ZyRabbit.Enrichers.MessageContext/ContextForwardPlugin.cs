using ZyRabbit.DependencyInjection;
using ZyRabbit.Enrichers.MessageContext.Dependencies;
using ZyRabbit.Enrichers.MessageContext.Middleware;
using ZyRabbit.Instantiation;

namespace ZyRabbit.Enrichers.MessageContext
{
	public static class ContextForwardPlugin
	{
		public static IClientBuilder UseContextForwarding(this IClientBuilder builder)
		{
			builder.Register(
				pipe => pipe
					.Use<PublishForwardingMiddleware>()
					.Use<ConsumeForwardingMiddleware>(),
				ioc => ioc
					.AddSingleton<IMessageContextRepository, MessageContextRepository>());
			return builder;
		}
	}
}
