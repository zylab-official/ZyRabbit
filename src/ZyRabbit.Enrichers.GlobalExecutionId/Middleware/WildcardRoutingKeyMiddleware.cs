using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit.Enrichers.GlobalExecutionId.Middleware
{
	public class WildcardRoutingKeyOptions
	{
		public Func<IPipeContext, bool> EnableRoutingFunc { get; set; }
		public Func<IPipeContext, string> ExecutionIdFunc { get; set; }
		public Func<IPipeContext, string, string> UpdateAction { get; set; }
	}

	public class WildcardRoutingKeyMiddleware : StagedMiddleware
	{
		public override string StageMarker => Pipe.StageMarker.ConsumeConfigured;
		protected Func<IPipeContext, bool> EnableRoutingFunc;
		protected Func<IPipeContext, string> ExecutionIdFunc;
		protected Func<IPipeContext, string, string> UpdateAction;
		protected readonly ILogger<WildcardRoutingKeyMiddleware> Logger;

		public WildcardRoutingKeyMiddleware(ILogger<WildcardRoutingKeyMiddleware> logger, WildcardRoutingKeyOptions options = null)
		{
			EnableRoutingFunc = options?.EnableRoutingFunc ?? (c => c.GetWildcardRoutingSuffixActive());
			ExecutionIdFunc = options?.ExecutionIdFunc ?? (c => c.GetGlobalExecutionId());
			UpdateAction = options?.UpdateAction ?? ((context, executionId) =>
			{
				var consumeConfig = context.GetConsumeConfiguration();
				if (consumeConfig != null)
				{
					consumeConfig.RoutingKey = $"{consumeConfig.RoutingKey}.#";
					return consumeConfig.RoutingKey;
				}
				return string.Empty;
			});
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public override Task InvokeAsync(IPipeContext context, CancellationToken token = new CancellationToken())
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
			Logger.LogInformation("Routing key updated with GlobalExecutionId: {globalExecutionId}", updated);
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

	public static class WildcardRoutingKeyExtensions
	{
		private const string SubscribeWithWildCard = "SubscribeWithWildCard";

		public static TPipeContext UseWildcardRoutingSuffix<TPipeContext>(this TPipeContext context, bool withWildCard = true) where TPipeContext : IPipeContext
		{
			context.Properties.AddOrReplace(SubscribeWithWildCard, withWildCard);
			return context;
		}

		public static bool GetWildcardRoutingSuffixActive(this IPipeContext context)
		{
			return context.Get(SubscribeWithWildCard, true);
		}
	}
}
