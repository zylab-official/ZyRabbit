using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit.Enrichers.HttpContext
{
	public class NetFxHttpContextMiddleware : StagedMiddleware
	{
		public override string StageMarker => Pipe.StageMarker.Initialized;

		public override Task InvokeAsync(IPipeContext context, CancellationToken token = new CancellationToken())
		{
#if NET451
			context.UseHttpContext(System.Web.HttpContext.Current);
#endif
			return Next.InvokeAsync(context, token);
		}
	}
}
