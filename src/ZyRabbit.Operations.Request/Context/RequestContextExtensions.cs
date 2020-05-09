using System;
using ZyRabbit.Operations.Request.Configuration.Abstraction;
using ZyRabbit.Operations.Request.Context;
using ZyRabbit.Pipe;

namespace ZyRabbit
{
	public static class RequestContextExtensions
	{
		public static IRequestContext UseRequestConfiguration(this IRequestContext context, Action<IRequestConfigurationBuilder> configuration)
		{
			context.Properties.Add(PipeKey.ConfigurationAction, configuration);
			return context;
		}
	}
}
