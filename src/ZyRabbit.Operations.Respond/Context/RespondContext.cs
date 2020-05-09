using ZyRabbit.Pipe;

namespace ZyRabbit.Operations.Respond.Context
{
	public interface IRespondContext : IPipeContext { }

	public class RespondContext : PipeContext, IRespondContext
	{
		public RespondContext(IPipeContext context)
		{
			Properties = context?.Properties;
		}
	}
}
