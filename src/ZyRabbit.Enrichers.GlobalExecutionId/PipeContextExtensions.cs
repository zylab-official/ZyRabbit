using ZyRabbit.Pipe;

namespace ZyRabbit.Enrichers.GlobalExecutionId
{
	public static class PipeContextExtensions
	{
		public static string GetGlobalExecutionId(this IPipeContext context)
		{
			return context.Get<string>(PipeKey.GlobalExecutionId);
		}
	}
}
