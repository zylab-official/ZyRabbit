using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Common;
using ZyRabbit.Configuration.Queue;

namespace ZyRabbit.Pipe.Middleware
{
	public class QueueDeclareOptions
	{
		public Func<IPipeContext, QueueDeclaration> QueueDeclarationFunc { get; set; }
	}

	public class QueueDeclareMiddleware : Middleware
	{
		protected readonly Func<IPipeContext, QueueDeclaration> QueueDeclareFunc;
		protected readonly ITopologyProvider Topology;
		protected readonly ILogger<QueueDeclareMiddleware> Logger;

		public QueueDeclareMiddleware(ITopologyProvider topology, ILogger<QueueDeclareMiddleware> logger, QueueDeclareOptions options = null )
		{
			Topology = topology ?? throw new ArgumentNullException(nameof(topology));
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			QueueDeclareFunc = options?.QueueDeclarationFunc ?? (context => context.GetQueueDeclaration());
		}

		public override async Task InvokeAsync(IPipeContext context, CancellationToken token = default (CancellationToken))
		{
			var queue = GetQueueDeclaration(context);

			if (queue != null)
			{
				Logger.LogDebug("Declaring queue '{queueName}'.", queue.Name);
				await DeclareQueueAsync(queue, context, token);
			}
			else
			{
				Logger.LogWarning("Queue will not be declaired: no queue declaration found in context.");
			}

			await Next.InvokeAsync(context, token);
		}

		protected virtual QueueDeclaration GetQueueDeclaration(IPipeContext context)
		{
			return QueueDeclareFunc(context);
		}

		protected virtual Task DeclareQueueAsync(QueueDeclaration queue, IPipeContext context, CancellationToken token)
		{
			return Topology.DeclareQueueAsync(queue);
		}
	}
}
