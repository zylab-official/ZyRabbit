using Microsoft.AspNetCore.Http;
using ZyRabbit.Enrichers.HttpContext;
using ZyRabbit.Instantiation;

namespace ZyRabbit
{
	public static class HttpContextPlugin
	{
		public static IClientBuilder UseHttpContext(this IClientBuilder builder)
		{
			builder.Register(
				p => p.Use<AspNetCoreHttpContextMiddleware>(),
				p => p.AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
			);
			return builder;
		}
	}
}
