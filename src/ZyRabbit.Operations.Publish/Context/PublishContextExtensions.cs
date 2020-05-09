using System;
using ZyRabbit.Configuration.Publisher;
using ZyRabbit.Operations.Publish.Context;
using ZyRabbit.Pipe;

namespace ZyRabbit
{
	public static class PublishContextExtensions
	{
		public static IPublishContext UsePublishConfiguration(this IPublishContext context, Action<IPublisherConfigurationBuilder> configuration)
		{
			context.Properties.Add(PipeKey.ConfigurationAction, configuration);
			return context;
		}
	}
}
