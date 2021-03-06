using System;

namespace ZyRabbit.Configuration.Publisher
{
	public interface IPublisherConfigurationFactory
	{
		PublisherConfiguration Create<TMessage>();
		PublisherConfiguration Create(Type messageType);
		PublisherConfiguration Create(string exchangeName, string routingKey);
	}
}
