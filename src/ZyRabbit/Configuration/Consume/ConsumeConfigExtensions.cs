using System;
using ZyRabbit.Configuration.Queue;

namespace ZyRabbit.Configuration.Consume
{
	public static class ConsumeConfigExtensions
	{
		public static bool IsDirectReplyTo(this ConsumeConfiguration cfg)
		{
			return string.Equals(cfg.QueueName, QueueDecclarationExtensions.DirectQueueName, StringComparison.CurrentCultureIgnoreCase);
		}
	}
}
