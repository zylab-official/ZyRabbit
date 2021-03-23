using System;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Operations.StateMachine.Core;
using ZyRabbit.Pipe;

namespace ZyRabbit.Operations.StateMachine.Middleware
{
	public class PersistModelMiddleware : Pipe.Middleware.Middleware
	{
		private readonly IStateMachineActivator _stateMachineActivator;

		public PersistModelMiddleware(IStateMachineActivator stateMachineActivator)
		{
			_stateMachineActivator = stateMachineActivator ?? throw new ArgumentNullException(nameof(stateMachineActivator));
		}

		public override async Task InvokeAsync(IPipeContext context, CancellationToken token)
		{
			var machine = context.GetStateMachine();
			await _stateMachineActivator.PersistAsync(machine);
			await Next.InvokeAsync(context, token);
		}
	}
}
