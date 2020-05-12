using System;
using RabbitMQ.Client;
using ZyRabbit.Configuration.Consume;
using ZyRabbit.Operations.Request.Core;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;
using ZyRabbit.Serialization;

namespace ZyRabbit.Operations.Request.Middleware
{
	public class BasicPropertiesMiddleware : Pipe.Middleware.BasicPropertiesMiddleware
	{
		public BasicPropertiesMiddleware(ISerializer serializer, BasicPropertiesOptions options) :base(serializer, options)
		{ }

		protected override void ModifyBasicProperties(IPipeContext context, IBasicProperties props)
		{
			var correlationId = context.GetCorrelationId() ?? Guid.NewGuid().ToString();
			var consumeCfg = context.GetResponseConfiguration();
			var clientCfg = context.GetClientConfiguration();

			if (consumeCfg.Consume.IsDirectReplyTo() || consumeCfg.Exchange == null)
			{
				props.ReplyTo = consumeCfg.Consume.QueueName;
			}
			else
			{
				props.ReplyToAddress = new PublicationAddress(consumeCfg.Exchange.ExchangeType, consumeCfg.Exchange.Name, consumeCfg.Consume.RoutingKey);
			}

			props.CorrelationId = correlationId;
			props.Expiration = clientCfg.RequestTimeout.TotalMilliseconds.ToString();
		}
	}
}
