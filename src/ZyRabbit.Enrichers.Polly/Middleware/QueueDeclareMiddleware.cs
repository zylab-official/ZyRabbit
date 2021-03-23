using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Common;
using ZyRabbit.Configuration.Queue;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit.Enrichers.Polly.Middleware
{
	public class QueueDeclareMiddleware : Pipe.Middleware.QueueDeclareMiddleware
	{
		public QueueDeclareMiddleware(ITopologyProvider topology, ILogger<Pipe.Middleware.QueueDeclareMiddleware> logger, QueueDeclareOptions options = null)
				: base(topology, logger, options)
		{
		}

		protected override Task DeclareQueueAsync(QueueDeclaration queue, IPipeContext context, CancellationToken token)
		{
			var policy = context.GetPolicy(PolicyKeys.QueueDeclare);
			return policy.ExecuteAsync(
				action: ct => base.DeclareQueueAsync(queue, context, ct),
				cancellationToken: token,
				contextData: new Dictionary<string, object>
				{
					[RetryKey.TopologyProvider] = Topology,
					[RetryKey.QueueDeclaration] = queue,
					[RetryKey.PipeContext] = context,
					[RetryKey.CancellationToken] = token,
				});
		}
	}
}
