using System.Collections.Generic;
using ZyRabbit.Operations.MessageSequence.Model;
using ZyRabbit.Operations.StateMachine;

namespace ZyRabbit.Operations.MessageSequence.StateMachine
{
	public class SequenceModel : Model<SequenceState>
	{
		public bool Aborted { get; set; }
		public List<ExecutionResult> Completed { get; set; }
		public List<ExecutionResult> Skipped { get; set; }
	}
}
