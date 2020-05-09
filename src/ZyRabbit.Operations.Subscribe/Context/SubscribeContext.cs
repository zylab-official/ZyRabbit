using ZyRabbit.Pipe;

namespace ZyRabbit.Operations.Subscribe.Context
{
	public interface ISubscribeContext : IPipeContext { }

	public class SubscribeContext : PipeContext, ISubscribeContext
	{
		public SubscribeContext(IPipeContext context)
		{
			Properties = context.Properties;
		}
	}
}
