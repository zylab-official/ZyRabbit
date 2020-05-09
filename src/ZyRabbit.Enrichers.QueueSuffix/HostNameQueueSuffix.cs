using System;
using ZyRabbit.Instantiation;

namespace ZyRabbit.Enrichers.QueueSuffix
{
	public static class HostNameQueueSuffix
	{
		public static IClientBuilder UseHostQueueSuffix(this IClientBuilder builder)
		{
			builder.UseQueueSuffix(new QueueSuffixOptions
			{
				CustomSuffixFunc = context => Environment.MachineName.ToLower(),
				ActiveFunc = context => context.GetHostnameQueueSuffixFlag()
			});

			return builder;
		}
	}
}
