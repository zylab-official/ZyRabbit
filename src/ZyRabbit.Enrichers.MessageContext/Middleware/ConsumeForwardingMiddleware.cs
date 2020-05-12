using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Enrichers.MessageContext.Dependencies;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit.Enrichers.MessageContext.Middleware
{
	public class ConsumeForwardingMiddleware : StagedMiddleware
	{
		private readonly IMessageContextRepository _repo;

		public ConsumeForwardingMiddleware(IMessageContextRepository repo)
		{
			_repo = repo;
		}

		public override Task InvokeAsync(IPipeContext context, CancellationToken token)
		{
			var messageContext = context.GetMessageContext();
			if (messageContext != null)
			{
				_repo.Set(messageContext);
			}
			return Next.InvokeAsync(context, token);
		}

		public override string StageMarker => "MessageContextDeserialized";
	}
}
