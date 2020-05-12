using ZyRabbit.Compatibility.Legacy.Configuration.Exchange;
using ZyRabbit.Compatibility.Legacy.Configuration.Queue;
using ZyRabbit.Compatibility.Legacy.Configuration.Respond;

namespace ZyRabbit.Compatibility.Legacy.Configuration.Request
{
	public class RequestConfiguration : IConsumerConfiguration
	{
		public ExchangeConfiguration Exchange { get; set; }
		public string RoutingKey { get; set; }

		/* Response Queue Configuration*/
		public bool AutoAck { get; set; }
		public ushort PrefetchCount => 1; // Only expect one response
		public QueueConfiguration Queue => ReplyQueue;
		public QueueConfiguration ReplyQueue { get; set; }
		public string ReplyQueueRoutingKey { get; set; }

		public RequestConfiguration()
		{
			Exchange = new ExchangeConfiguration();
			ReplyQueue = new QueueConfiguration();
			AutoAck = true;
		}
	}
}
