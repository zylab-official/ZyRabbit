using System;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Operations.StateMachine.Core;
using ZyRabbit.Pipe;

namespace ZyRabbit.Operations.StateMachine.Middleware
{
	public class GlobalLockMiddleware : Pipe.Middleware.Middleware
	{
		private readonly IGlobalLock _globalLock;

		public GlobalLockMiddleware(IGlobalLock globalLock)
		{
			_globalLock = globalLock;
		}

		public override Task InvokeAsync(IPipeContext context, CancellationToken token)
		{
			return _globalLock.ExecuteAsync(context.Get<Guid>(StateMachineKey.ModelId), () => Next.InvokeAsync(context, token), token);
		}
	}
}
