using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Enrichers.GlobalExecutionId.Dependencies;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit.Enrichers.GlobalExecutionId.Middleware
{
	public class AppendGlobalExecutionIdOptions
	{
		public Func<IPipeContext, string> ExecutionIdFunc { get; set; }
		public Action<IPipeContext, string> SaveInContext { get; set; }
	}

	public class AppendGlobalExecutionIdMiddleware : StagedMiddleware
	{
		public override string StageMarker => Pipe.StageMarker.ProducerInitialized;
		protected Func<IPipeContext, string> ExecutionIdFunc;
		protected Action<IPipeContext, string> SaveInContext;
		protected readonly ILogger<AppendGlobalExecutionIdMiddleware> Logger;

		public AppendGlobalExecutionIdMiddleware(ILogger<AppendGlobalExecutionIdMiddleware> logger, AppendGlobalExecutionIdOptions options = null)
		{
			ExecutionIdFunc = options?.ExecutionIdFunc ?? (context => context.GetGlobalExecutionId());
			SaveInContext = options?.SaveInContext ?? ((ctx, id) => ctx.Properties.TryAdd(PipeKey.GlobalExecutionId, id));
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public override Task InvokeAsync(IPipeContext context, CancellationToken token = new CancellationToken())
		{
			var fromContext = GetExecutionIdFromContext(context);
			if (!string.IsNullOrWhiteSpace(fromContext))
			{
				Logger.LogInformation("GlobalExecutionId {globalExecutionId} was allready found in PipeContext.", fromContext);
				return Next.InvokeAsync(context, token);
			}
			var fromProcess = GetExecutionIdFromProcess();
			if (!string.IsNullOrWhiteSpace(fromProcess))
			{
				Logger.LogInformation("Using GlobalExecutionId {globalExecutionId} that was found in the execution process.", fromProcess);
				AddToContext(context, fromProcess);
				return Next.InvokeAsync(context, token);
			}
			var created = CreateExecutionId(context);
			AddToContext(context, created);
			Logger.LogInformation("Creating new GlobalExecutionId {globalExecutionId} for this execution.", created);
			return Next.InvokeAsync(context, token);
		}

		protected virtual void AddToContext(IPipeContext context, string globalMessageId)
		{
			SaveInContext(context, globalMessageId);
		}

		protected virtual string CreateExecutionId(IPipeContext context)
		{
			return  Guid.NewGuid().ToString();
		}

		protected virtual string GetExecutionIdFromProcess()
		{
			return GlobalExecutionIdRepository.Get();
		}

		protected virtual string GetExecutionIdFromContext(IPipeContext context)
		{
			return ExecutionIdFunc(context);
		}
	}
}
