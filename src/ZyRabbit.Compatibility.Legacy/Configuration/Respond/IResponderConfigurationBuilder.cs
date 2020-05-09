using System;
using ZyRabbit.Compatibility.Legacy.Configuration.Exchange;
using ZyRabbit.Compatibility.Legacy.Configuration.Queue;

namespace ZyRabbit.Compatibility.Legacy.Configuration.Respond
{
	public interface IResponderConfigurationBuilder
	{
		IResponderConfigurationBuilder WithPrefetchCount(ushort count);
		IResponderConfigurationBuilder WithExchange(Action<IExchangeConfigurationBuilder> exchange);
		IResponderConfigurationBuilder WithQueue(Action<IQueueConfigurationBuilder> queue);
		IResponderConfigurationBuilder WithRoutingKey(string routingKey);
		[Obsolete("Property name changed. Use 'WithAutoAck' instead.")]
		IResponderConfigurationBuilder WithNoAck(bool noAck);
		IResponderConfigurationBuilder WithAutoAck(bool autoAck = true);
	}
}
