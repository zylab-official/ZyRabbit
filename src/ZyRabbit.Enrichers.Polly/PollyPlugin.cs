using System;
using ZyRabbit.Channel.Abstraction;
using ZyRabbit.Instantiation;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit
{
	public static class PollyPlugin
	{
		public static IClientBuilder UsePolly(this IClientBuilder builder, Action<IPipeContext> action)
		{
			return UsePolly(builder, new PolicyOptions {PolicyAction = action});
		}

		public static IClientBuilder UsePolly(this IClientBuilder builder, PolicyOptions options)
		{
			builder.Register(
				pipe => pipe
					.Use<PolicyMiddleware>(options)
					.Replace<QueueDeclareMiddleware, Enrichers.Polly.Middleware.QueueDeclareMiddleware>(argsFunc: oldArgs => oldArgs)
					.Replace<ExchangeDeclareMiddleware, Enrichers.Polly.Middleware.ExchangeDeclareMiddleware>(argsFunc: oldArgs => oldArgs)
					.Replace<QueueBindMiddleware, Enrichers.Polly.Middleware.QueueBindMiddleware>(argsFunc: oldArgs => oldArgs)
					.Replace<ConsumerCreationMiddleware, Enrichers.Polly.Middleware.ConsumerCreationMiddleware>(argsFunc: oldArgs => oldArgs)
					.Replace<BasicPublishMiddleware, Enrichers.Polly.Middleware.BasicPublishMiddleware>(argsFunc: oldArgs => oldArgs)
					.Replace<ExplicitAckMiddleware, Enrichers.Polly.Middleware.ExplicitAckMiddleware>(argsFunc: oldArgs => oldArgs)
					.Replace<PooledChannelMiddleware, Enrichers.Polly.Middleware.PooledChannelMiddleware>(argsFunc: oldArgs => oldArgs)
					.Replace<TransientChannelMiddleware, Enrichers.Polly.Middleware.TransientChannelMiddleware>(argsFunc: oldArgs => oldArgs)
					.Replace<HandlerInvocationMiddleware, Enrichers.Polly.Middleware.HandlerInvocationMiddleware>(argsFunc: oldArgs => oldArgs),
				ioc => ioc
					.AddSingleton<IChannelFactory, Enrichers.Polly.Services.ChannelFactory>()
					.AddSingleton(options.ConnectionPolicies));
			return builder;
		}
	}
}
