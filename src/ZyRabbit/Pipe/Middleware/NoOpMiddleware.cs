using System.Threading;
using System.Threading.Tasks;

namespace ZyRabbit.Pipe.Middleware
{
	public class NoOpMiddleware : Middleware
	{
		public override Task InvokeAsync(IPipeContext context, CancellationToken token)
		{
			return Task.FromResult(0);
		}
	}
}
