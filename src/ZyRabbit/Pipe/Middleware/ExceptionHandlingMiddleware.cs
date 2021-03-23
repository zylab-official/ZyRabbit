using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZyRabbit.Pipe.Middleware
{
	public class ExceptionHandlingOptions
	{
		public Func<Exception, IPipeContext, CancellationToken, Task> HandlingFunc { get; set; }

		public Action<IPipeBuilder> InnerPipe { get; set; }
	}

	public class ExceptionHandlingMiddleware : Middleware
	{
		protected readonly Func<Exception, IPipeContext, CancellationToken, Task> HandlingFunc;
		public Middleware InnerPipe;
		protected readonly ILogger<ExceptionHandlingMiddleware> Logger;

		public ExceptionHandlingMiddleware(IPipeBuilderFactory factory, ILogger<ExceptionHandlingMiddleware> logger, ExceptionHandlingOptions options = null)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			HandlingFunc = options?.HandlingFunc ?? ((exception, context, token) => Task.CompletedTask);
			InnerPipe = (factory ?? throw new ArgumentNullException(nameof(factory))).Create(options?.InnerPipe);
		}

		public override async Task InvokeAsync(IPipeContext context, CancellationToken token)
		{
			try
			{
				await InnerPipe.InvokeAsync(context, token);
				await Next.InvokeAsync(context, token);
			}
			catch (Exception e)
			{
				Logger.LogError(e, "Exception thrown. Will be handled by Exception Handler");
				await OnExceptionAsync(e, context, token);
			}
		}

		protected virtual Task OnExceptionAsync(Exception exception, IPipeContext context, CancellationToken token)
		{
			return HandlingFunc(exception, context, token);
		}

		protected static Exception UnwrapInnerException(Exception exception)
		{
			if (exception is AggregateException && exception.InnerException != null)
			{
				return UnwrapInnerException(exception.InnerException);
			}
			return exception;
		}
	}
}
