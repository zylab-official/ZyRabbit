using ZyRabbit.Common;
using ZyRabbit.DependencyInjection;
using ZyRabbit.Instantiation;
using ZyRabbit.Middleware;

namespace ZyRabbit
{
	public static class RetryLaterPlugin
	{
		public static IClientBuilder UseRetryLater(this IClientBuilder builder)
		{
			builder.Register(
				pipe => pipe
					.Use<RetryLaterMiddleware>()
					.Use<RetryInformationExtractionMiddleware>(),
				ioc => ioc
					.AddSingleton<IRetryInformationHeaderUpdater, RetryInformationHeaderUpdater>()
					.AddSingleton<IRetryInformationProvider, RetryInformationProvider>()
				);
			return builder;
		}
	}
}
