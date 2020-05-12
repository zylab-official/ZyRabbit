using System;
using ZyRabbit.Configuration.Consume;
using ZyRabbit.Configuration.Queue;
using ZyRabbit.Pipe;

namespace ZyRabbit.Enrichers.QueueSuffix
{
	public class QueueSuffixOptions
	{
		public Func<IPipeContext, QueueDeclaration> QueueDeclareFunc;
		public Func<IPipeContext, string> CustomSuffixFunc;
		public Func<IPipeContext, string> ContextSuffixOverrideFunc;
		public Func<IPipeContext, bool> ActiveFunc;
		public Func<string, bool> SkipSuffixFunc;
		public Func<IPipeContext, ConsumeConfiguration> ConsumeConfigFunc;
		public Action<QueueDeclaration, string> AppendSuffixAction;
	}
}
