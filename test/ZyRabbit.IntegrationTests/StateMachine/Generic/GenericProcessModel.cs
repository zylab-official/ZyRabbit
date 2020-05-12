using System;
using ZyRabbit.Operations.StateMachine;

namespace ZyRabbit.IntegrationTests.StateMachine.Generic
{
	public class GenericProcessModel : Model<State>
	{
		public string Assignee { get; set; }
		public string Name { get; set; }
		public DateTime Deadline { get; set; }
	}
}
