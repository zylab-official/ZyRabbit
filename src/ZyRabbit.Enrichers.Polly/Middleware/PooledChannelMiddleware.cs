using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using ZyRabbit.Channel;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit.Enrichers.Polly.Middleware
{
	public class PooledChannelMiddleware : Pipe.Middleware.PooledChannelMiddleware
	{
		public PooledChannelMiddleware(IChannelPoolFactory poolFactory, PooledChannelOptions options = null)
			: base(poolFactory, options) { }

		protected override Task<IModel> GetChannelAsync(IPipeContext context, CancellationToken token)
		{
			var policy = context.GetPolicy(PolicyKeys.ChannelCreate);
			return policy.ExecuteAsync(
				action: ct => base.GetChannelAsync(context, ct),
				cancellationToken: token,
				contextData: new Dictionary<string, object>
				{
					[RetryKey.PipeContext] = context,
					[RetryKey.CancellationToken] = token,
					[RetryKey.ChannelPoolFactory] = PoolFactory
				}
			);
		}
	}
}
