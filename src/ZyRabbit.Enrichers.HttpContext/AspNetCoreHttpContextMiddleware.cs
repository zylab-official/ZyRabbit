using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit.Enrichers.HttpContext
{
	public class AspNetCoreHttpContextMiddleware : StagedMiddleware
	{
		private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpAccessor;

		public AspNetCoreHttpContextMiddleware(Microsoft.AspNetCore.Http.IHttpContextAccessor httpAccessor)
		{
			_httpAccessor = httpAccessor;
		}

		public override string StageMarker => Pipe.StageMarker.Initialized;

		public override Task InvokeAsync(IPipeContext context, CancellationToken token = new CancellationToken())
		{
			context.UseHttpContext(_httpAccessor.HttpContext);
			return Next.InvokeAsync(context, token);
		}

	}
}
