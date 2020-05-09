using System;

namespace ZyRabbit.Enrichers.MessageContext.Context
{
	public class MessageContext : IMessageContext
	{
		public Guid GlobalRequestId { get; set; }
	}
}
