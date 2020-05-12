using System;
using ZyRabbit.Enrichers.MessageContext.Context;

namespace ZyRabbit.IntegrationTests.TestMessages
{
	public class TestMessageContext : IMessageContext
	{
		public string Prop { get; set; }
		public Guid GlobalRequestId { get; set; }
	}
}
