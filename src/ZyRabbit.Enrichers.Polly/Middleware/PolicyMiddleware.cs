using System;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Enrichers.Polly.Services;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit
{
	public class PolicyOptions
	{
		public Action<IPipeContext> PolicyAction { get; set; }
		public ConnectionPolicies ConnectionPolicies { get; set; }
	}

	public class PolicyMiddleware : StagedMiddleware
	{
		protected Action<IPipeContext> PolicyAction;

		public PolicyMiddleware(PolicyOptions options = null)
		{
			PolicyAction = options?.PolicyAction;
		}

		public override Task InvokeAsync(IPipeContext context, CancellationToken token = new CancellationToken())
		{
			AddPolicies(context);
			return Next.InvokeAsync(context, token);
		}

		protected virtual void AddPolicies(IPipeContext context)
		{
			PolicyAction?.Invoke(context);
		}

		public override string StageMarker => Pipe.StageMarker.Initialized;
	}
}
