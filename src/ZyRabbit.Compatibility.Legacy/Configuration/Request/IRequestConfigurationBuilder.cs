using System;
using ZyRabbit.Compatibility.Legacy.Configuration.Exchange;
using ZyRabbit.Compatibility.Legacy.Configuration.Queue;

namespace ZyRabbit.Compatibility.Legacy.Configuration.Request
{
	public interface IRequestConfigurationBuilder
	{
		IRequestConfigurationBuilder WithExchange(Action<IExchangeConfigurationBuilder> exchange);
		IRequestConfigurationBuilder WithRoutingKey(string routingKey);
		IRequestConfigurationBuilder WithReplyQueue(Action<IQueueConfigurationBuilder> replyTo);
		[Obsolete("Property name changed. Use 'WithAutoAck' instead.")]
		IRequestConfigurationBuilder WithNoAck(bool noAck);
		IRequestConfigurationBuilder WithAutoAck(bool autoAck);
	}
}
