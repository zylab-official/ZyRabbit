using System;
using ZyRabbit.Common;
using ZyRabbit.Instantiation;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit.Enrichers.MessageContext
{
	public static class MessageContextPlugin
	{
		public static IClientBuilder UseMessageContext<TMessageContext>(this IClientBuilder builder)
			where TMessageContext : new()
		{
			return UseMessageContext(builder, context => new TMessageContext());
		}

		public static IClientBuilder UseMessageContext<TMessageContext>(this IClientBuilder builder, Func<IPipeContext, TMessageContext> createFunc)
		{
			Func<IPipeContext, object> genericCreateFunc = context => createFunc(context);
			builder.Register(pipe => pipe.Use<HeaderSerializationMiddleware>(new HeaderSerializationOptions
			{
				HeaderKeyFunc = context => PropertyHeaders.Context,
				RetrieveItemFunc = context => context.GetMessageContext(),
				CreateItemFunc = genericCreateFunc
			}));
			return builder;
		}
	}
}
