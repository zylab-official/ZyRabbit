using System.Collections.Generic;
using ZyRabbit.Configuration.Consume;
using ZyRabbit.Configuration.Exchange;
using ZyRabbit.Configuration.Queue;

namespace ZyRabbit.Configuration.Consumer
{
	public class ConsumerConfiguration
	{
		public QueueDeclaration Queue { get; set; }
		public ExchangeDeclaration Exchange { get; set; }
		public ConsumeConfiguration Consume { get; set; }
	}
}
