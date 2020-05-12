using System;
using ZyRabbit.Operations.Respond.Configuration;
using ZyRabbit.Operations.Respond.Context;
using ZyRabbit.Pipe;

namespace ZyRabbit
{
	public static class RespondContextExtensions
	{
		public static IRespondContext UseRespondConfiguration(this IRespondContext context, Action<IRespondConfigurationBuilder> configuration)
		{
			context.Properties.Add(PipeKey.ConfigurationAction, configuration);
			return context;
		}
	}
}
