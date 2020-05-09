using System;
using ZyRabbit.Pipe;

namespace ZyRabbit.Enrichers.MessageContext.Respond
{
	public static class PipeContextExtensions
	{
		private const string MessageContextType = "Respond:MessageContext:Type";

		public static IPipeContext AddMessageContextType<TMessageContext>(this IPipeContext context)
		{
			context.Properties.TryAdd(MessageContextType, typeof(TMessageContext));
			return context;
		}

		public static Type GetMessageContextType(this IPipeContext context)
		{
			return context.Get(MessageContextType, typeof(object));
		}
	}
}
