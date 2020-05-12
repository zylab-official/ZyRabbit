using System;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Enrichers.GlobalExecutionId.Dependencies;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit.Enrichers.GlobalExecutionId.Middleware
{
	public class PersistGlobalExecutionIdOptions
	{
		public Func<IPipeContext, string> ExecutionIdFunc { get; set; }
	}

	public class PersistGlobalExecutionIdMiddleware : StagedMiddleware
	{
		protected Func<IPipeContext, string> ExecutionIdFunc;

		public override string StageMarker => Pipe.StageMarker.MessageReceived;

		public PersistGlobalExecutionIdMiddleware(PersistGlobalExecutionIdOptions options = null)
		{
			ExecutionIdFunc = options?.ExecutionIdFunc ?? (context => context.GetGlobalExecutionId());
		}

		public override Task InvokeAsync(IPipeContext context, CancellationToken token = new CancellationToken())
		{
			var globalExecutionId = GetGlobalExecutionId(context);
			PersistInProcess(globalExecutionId);
			return Next.InvokeAsync(context, token);
		}

		protected virtual string GetGlobalExecutionId(IPipeContext context)
		{
			return ExecutionIdFunc(context);
		}

		protected virtual void PersistInProcess(string id)
		{
			GlobalExecutionIdRepository.Set(id);
		}
	}
}
