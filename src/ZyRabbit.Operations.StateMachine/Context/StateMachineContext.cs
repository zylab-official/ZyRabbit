using ZyRabbit.Pipe;

namespace ZyRabbit.Operations.StateMachine.Context
{
	public interface IStateMachineContext : IPipeContext { }

	public class StateMachineContext : PipeContext, IStateMachineContext
	{
		public StateMachineContext(IPipeContext context)
		{
			Properties = context?.Properties;
		}
	}
}
