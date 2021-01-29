using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit.Enrichers.GlobalExecutionId.Middleware
{
	public class ExecutionIdRoutingOptions
	{
		public Func<IPipeContext, bool> EnableRoutingFunc { get; set; }
		public Func<IPipeContext, string> ExecutionIdFunc { get; set; }
		public Func<IPipeContext, string, string> UpdateAction { get; set; }
	}

	public class ExecutionIdRoutingMiddleware : StagedMiddleware
	{
		public override string StageMarker => Pipe.StageMarker.PublishConfigured;
		protected Func<IPipeContext, bool> EnableRoutingFunc;
		protected Func<IPipeContext, string> ExecutionIdFunc;
		protected Func<IPipeContext, string, string> UpdateAction;
		protected readonly ILogger<ExecutionIdRoutingMiddleware> Logger;

		public ExecutionIdRoutingMiddleware(ILogger<ExecutionIdRoutingMiddleware> logger, ExecutionIdRoutingOptions options = null)
		{
			EnableRoutingFunc = options?.EnableRoutingFunc ?? (c => c.GetClientConfiguration()?.RouteWithGlobalId ?? false);
			ExecutionIdFunc = options?.ExecutionIdFunc ?? (c => c.GetGlobalExecutionId());
			UpdateAction = options?.UpdateAction ?? ((context, executionId) =>
			{
				var pubConfig = context.GetBasicPublishConfiguration();
				if (pubConfig != null)
				{
					pubConfig.RoutingKey = $"{pubConfig.RoutingKey}.{executionId}";
					return pubConfig.RoutingKey;
				}
				return string.Empty;
			});
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public override Task InvokeAsync(IPipeContext context, CancellationToken token)
		{
			var enabled = GetRoutingEnabled(context);
			if (!enabled)
			{
				Logger.LogDebug("Routing with GlobalExecutionId disabled.");
				return Next.InvokeAsync(context, token);
			}
			var executionId = GetExecutionId(context);
			UpdateRoutingKey(context, executionId);
			return Next.InvokeAsync(context, token);
		}

		protected virtual void UpdateRoutingKey(IPipeContext context, string executionId)
		{
			Logger.LogDebug("Updating routing key with GlobalExecutionId {globalExecutionId}", executionId);
			var updated = UpdateAction(context, executionId);
			Logger.LogInformation("Routing key updated with GlobalExecutionId: {routingKey}", updated);
		}

		protected virtual bool GetRoutingEnabled(IPipeContext pipeContext)
		{
			return EnableRoutingFunc(pipeContext);
		}

		protected virtual string GetExecutionId(IPipeContext context)
		{
			return ExecutionIdFunc(context);
		}
	}
}
