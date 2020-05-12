using ZyRabbit.Compatibility.Legacy.Configuration.Exchange;
using ZyRabbit.Compatibility.Legacy.Configuration.Queue;
using ZyRabbit.Compatibility.Legacy.Configuration.Respond;

namespace ZyRabbit.Compatibility.Legacy.Configuration.Subscribe
{
	public class SubscriptionConfiguration : IConsumerConfiguration
	{
		public bool AutoAck { get; set; }
		public ushort PrefetchCount { get; set; }
		public ExchangeConfiguration Exchange { get; set; }
		public QueueConfiguration Queue { get; set; }
		public string RoutingKey { get; set; }

		public SubscriptionConfiguration()
		{
			Exchange = new ExchangeConfiguration();
			Queue = new QueueConfiguration();
		}
	}
}
