using System;

namespace ZyRabbit.Configuration.Consume
{
	public interface IConsumeConfigurationFactory
	{
		ConsumeConfiguration Create<TMessage>();
		ConsumeConfiguration Create(Type messageType);
		ConsumeConfiguration Create(string queueName, string exchangeName, string routingKey);
	}
}
