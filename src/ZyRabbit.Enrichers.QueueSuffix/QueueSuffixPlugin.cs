using System;
using ZyRabbit.Instantiation;

namespace ZyRabbit.Enrichers.QueueSuffix
{
	public static class QueueSuffixPlugin
	{
		public static IClientBuilder UseQueueSuffix(this IClientBuilder builder, QueueSuffixOptions options = null)
		{
			if (options == null)
			{
				builder.Register(pipe => pipe.Use<QueueSuffixMiddleware>());
			}
			else
			{
				builder.Register(pipe => pipe.Use<QueueSuffixMiddleware>(options));
			}
			return builder;
		}
	}
}
