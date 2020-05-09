using System;

namespace ZyRabbit.Configuration.BasicPublish
{
	public interface IBasicPublishConfigurationFactory
	{
		BasicPublishConfiguration Create();
		BasicPublishConfiguration Create(Type type);
		BasicPublishConfiguration Create(object message);
	}
}
