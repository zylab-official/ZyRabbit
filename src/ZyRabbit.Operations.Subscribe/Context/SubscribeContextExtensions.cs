using System;
using ZyRabbit.Configuration.Consumer;
using ZyRabbit.Operations.Subscribe.Context;
using ZyRabbit.Pipe;

namespace ZyRabbit
{
	public static class SubscribeContextExtensions
	{
		public static ISubscribeContext UseSubscribeConfiguration(this ISubscribeContext context, Action<IConsumerConfigurationBuilder> configuration)
		{
			context.Properties.Add(PipeKey.ConfigurationAction, configuration);
			return context;
		}
	}
}
