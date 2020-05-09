using ZyRabbit.Operations.Respond.Acknowledgement;
using ZyRabbit.Operations.Respond.Core;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit.Operations.Respond.Middleware
{
	public class ResponseHandlerOptionFactory
	{
		public static HandlerInvocationOptions Create(HandlerInvocationOptions options = null)
		{
			return new HandlerInvocationOptions
			{
				HandlerArgsFunc = options?.HandlerArgsFunc ?? (context => new[] {context.GetMessage()}),
				PostInvokeAction = options?.PostInvokeAction ?? ((context, task) =>
				{
					if (task is Ack ack)
					{
						context.Properties.TryAdd(RespondKey.ResponseMessage, ack.Response);
					}
				})
			};
		}
	}
}
