using System;
using RabbitMQ.Client.Events;
using ZyRabbit.Configuration.BasicPublish;
using ZyRabbit.Configuration.Exchange;

namespace ZyRabbit.Configuration.Publisher
{
	public class PublisherConfiguration : BasicPublishConfiguration
	{
		public ExchangeDeclaration Exchange { get; set; }
		public EventHandler<BasicReturnEventArgs> ReturnCallback { get; set; }
	}
}
