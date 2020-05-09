using ZyRabbit.Pipe;

namespace ZyRabbit.Enrichers.MessageContext
{
	public static class PipeContextExtensions
	{
		public static TPipeContext UseMessageContext<TPipeContext>(this TPipeContext context, object msgContext) where TPipeContext : IPipeContext
		{
			context.Properties.TryAdd(PipeKey.MessageContext, msgContext);
			return context;
		}
	}
}
