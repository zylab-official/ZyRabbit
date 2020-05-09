using System;

namespace ZyRabbit.IntegrationTests.TestMessages
{
	public class BasicResponse
	{
		public string Prop { get; set; }
		public Guid Payload { get; set; }
	}
}
