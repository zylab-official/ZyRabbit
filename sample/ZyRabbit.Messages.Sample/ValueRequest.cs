using ZyRabbit.Configuration.Exchange;
using ZyRabbit.Enrichers.Attributes;

namespace ZyRabbit.Messages.Sample
{
	[Exchange(Type = ExchangeType.Topic, Name = "custom.rpc.exchange")]
	[Queue(Name = "custom.request.queue", Durable = false)]
	[Routing(RoutingKey = "custom.routing.key")]
	public class ValueRequest
	{
		public int Value { get; set; }
	}
}
