using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit.Operations.Subscribe.Middleware
{
	public class SubscribeInvocationMiddleware : HandlerInvocationMiddleware
	{
		public SubscribeInvocationMiddleware() : base(new HandlerInvocationOptions
		{
			HandlerArgsFunc = context => new []{ context.GetMessage()}
		})
		{ }
	}
}
