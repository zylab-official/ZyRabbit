using ZyRabbit.Pipe;

namespace ZyRabbit.Enrichers.HttpContext
{
	public static class PipeContextHttpExtensions
	{
		public const string HttpContext = "HttpContext";

		public static IPipeContext UseHttpContext(this IPipeContext pipeContext, Microsoft.AspNetCore.Http.HttpContext httpContext)
		{
			pipeContext.Properties.AddOrReplace(HttpContext, httpContext);
			return pipeContext;
		}

		public static Microsoft.AspNetCore.Http.HttpContext GetHttpContext(this IPipeContext pipeContext)
		{
			return pipeContext.Get<Microsoft.AspNetCore.Http.HttpContext>(HttpContext);
		}
	}
}
