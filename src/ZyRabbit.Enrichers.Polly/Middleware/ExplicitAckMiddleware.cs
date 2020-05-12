using System.Collections.Generic;
using ZyRabbit.Common;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;
using System.Threading.Tasks;
using ZyRabbit.Channel.Abstraction;

namespace ZyRabbit.Enrichers.Polly.Middleware
{
	public class ExplicitAckMiddleware : Pipe.Middleware.ExplicitAckMiddleware
	{
		public ExplicitAckMiddleware(INamingConventions conventions, ITopologyProvider topology, IChannelFactory channelFactory, ExplicitAckOptions options = null)
				: base(conventions, topology, channelFactory, options) { }

		protected override async Task<Acknowledgement> AcknowledgeMessageAsync(IPipeContext context)
		{
			var policy = context.GetPolicy(PolicyKeys.MessageAcknowledge);
			var result = await policy.ExecuteAsync(
				action: () => Task.FromResult(base.AcknowledgeMessageAsync(context)),
				contextData: new Dictionary<string, object>
				{
					[RetryKey.PipeContext] = context
				});
			return await result;
		}
	}
}
