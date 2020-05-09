using System;
using ZyRabbit.Configuration.Consumer;
using ZyRabbit.Configuration.Publisher;

namespace ZyRabbit.Operations.Request.Configuration.Abstraction
{
	public interface IRequestConfigurationBuilder
	{
		IRequestConfigurationBuilder PublishRequest(Action<IPublisherConfigurationBuilder> publish);
		IRequestConfigurationBuilder ConsumeResponse(Action<IConsumerConfigurationBuilder> consume);
	}
}
