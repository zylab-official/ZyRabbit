using System;

namespace ZyRabbit.Enrichers.MessageContext.Context
{
	public interface IMessageContext
	{
		Guid GlobalRequestId { get; set; }
	}
}
