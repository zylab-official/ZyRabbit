using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZyRabbit.Pipe.Middleware
{
	public class StageMarkerMiddleware : Middleware
	{
		public readonly string Stage;
		private readonly ILogger<StageMarkerMiddleware> Logger;

		public StageMarkerMiddleware(ILogger<StageMarkerMiddleware> logger, StageMarkerOptions options)
		{
			if (options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			Stage = options.Stage;
		}

		public override Task InvokeAsync(IPipeContext context, CancellationToken token)
		{
			if (Next is NoOpMiddleware || Next is CancellationMiddleware)
			{
				Logger.LogDebug("Stage {pipeStage} has no additional middlewares registered.", Stage);
			}
			else
			{
				Logger.LogInformation("Invoking additional middlewares on stage {pipeStage}", Stage);
			}
			return Next.InvokeAsync(context, token);
		}
	}

	public class StageMarkerOptions
	{
		public string Stage { get; set; }

		public static StageMarkerOptions For<TPipe>(TPipe stage)
		{
			return new StageMarkerOptions
			{
				Stage = stage.ToString()
			};
		}
	}


	public abstract class StagedMiddleware : Middleware
	{
		public abstract string StageMarker { get; }
	}
}
