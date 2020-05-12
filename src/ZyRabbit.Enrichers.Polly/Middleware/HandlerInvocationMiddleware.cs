using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit.Enrichers.Polly.Middleware
{
	public class HandlerInvocationMiddleware : Pipe.Middleware.HandlerInvocationMiddleware
	{
		public HandlerInvocationMiddleware(HandlerInvocationOptions options = null)
			: base(options) { }

		protected override Task InvokeMessageHandler(IPipeContext context, CancellationToken token)
		{
			var policy = context.GetPolicy(PolicyKeys.HandlerInvocation);
			return policy.ExecuteAsync(
				action: ct => base.InvokeMessageHandler(context, ct),
				cancellationToken: token,
				contextData: new Dictionary<string, object>
				{
					[RetryKey.PipeContext] = context,
					[RetryKey.CancellationToken] = token
				});
		}
	}
}
