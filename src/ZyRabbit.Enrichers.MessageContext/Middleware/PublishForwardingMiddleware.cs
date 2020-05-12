using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Enrichers.MessageContext.Dependencies;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit.Enrichers.MessageContext.Middleware
{
	public class PublishForwardingMiddleware : StagedMiddleware
	{
		private readonly IMessageContextRepository _repo;

		public PublishForwardingMiddleware(IMessageContextRepository repo)
		{
			_repo = repo;
		}

		public override Task InvokeAsync(IPipeContext context, CancellationToken token)
		{
			var messageContext = _repo.Get();
			if (messageContext == null)
			{
				return Next.InvokeAsync(context, token);
			}
			if (context.Properties.ContainsKey(PipeKey.MessageContext))
			{
				context.Properties.Remove(PipeKey.MessageContext);
			}
			context.Properties.Add(PipeKey.MessageContext, messageContext);
			return Next.InvokeAsync(context, token);
		}

		public override string StageMarker => Pipe.StageMarker.Initialized;
	}
}
