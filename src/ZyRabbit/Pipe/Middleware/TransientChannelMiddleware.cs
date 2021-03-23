using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using ZyRabbit.Channel.Abstraction;

namespace ZyRabbit.Pipe.Middleware
{
	public class TransientChannelMiddleware : Middleware
	{
		protected readonly IChannelFactory ChannelFactory;
		protected readonly ILogger<TransientChannelMiddleware> Logger;

		public TransientChannelMiddleware(IChannelFactory channelFactory, ILogger<TransientChannelMiddleware> logger)
		{
			ChannelFactory = channelFactory ?? throw new ArgumentNullException(nameof(channelFactory));
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public override async Task InvokeAsync(IPipeContext context, CancellationToken token)
		{
			using (var channel = await CreateChannelAsync(context, token))
			{
				Logger.LogDebug("Adding channel {channelNumber} to Execution Context.", channel.ChannelNumber);
				context.Properties.Add(PipeKey.TransientChannel, channel);
				await Next.InvokeAsync(context, token);
			}
		}

		protected virtual Task<IModel> CreateChannelAsync(IPipeContext context, CancellationToken ct)
		{
			return ChannelFactory.CreateChannelAsync(ct);
		}
	}
}
