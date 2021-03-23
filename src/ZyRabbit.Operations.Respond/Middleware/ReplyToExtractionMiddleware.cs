using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ZyRabbit.Operations.Respond.Core;
using ZyRabbit.Pipe;

namespace ZyRabbit.Operations.Respond.Middleware
{
	public class ReplyToExtractionOptions
	{
		public Func<IPipeContext, BasicDeliverEventArgs> DeliveryArgsFunc { get; set; }
		public Func<BasicDeliverEventArgs, PublicationAddress> ReplyToFunc { get; set; }
		public Action<IPipeContext, PublicationAddress> ContextSaveAction { get; set; }
	}

	public class ReplyToExtractionMiddleware : Pipe.Middleware.Middleware
	{
		protected readonly Func<IPipeContext, BasicDeliverEventArgs> DeliveryArgsFunc;
		protected readonly Func<BasicDeliverEventArgs, PublicationAddress> ReplyToFunc;
		protected readonly Action<IPipeContext, PublicationAddress> ContextSaveAction;
		protected readonly ILogger<ReplyToExtractionMiddleware> Logger;

		public ReplyToExtractionMiddleware(ILogger<ReplyToExtractionMiddleware> logger, ReplyToExtractionOptions options = null)
		{
			ContextSaveAction = options?.ContextSaveAction ?? ((ctx, addr) => ctx.Properties.Add(RespondKey.PublicationAddress, addr));
			DeliveryArgsFunc = options?.DeliveryArgsFunc ?? (ctx => ctx.GetDeliveryEventArgs());
			ReplyToFunc = options?.ReplyToFunc ?? (args =>
				args.BasicProperties.ReplyToAddress ?? new PublicationAddress(ExchangeType.Direct, string.Empty, args.BasicProperties.ReplyTo));
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public override Task InvokeAsync(IPipeContext context, CancellationToken token)
		{
			var args = GetDeliveryArgs(context);
			var replyTo = GetReplyTo(args);
			SaveInContext(context, replyTo);
			return Next.InvokeAsync(context, token);
		}

		protected virtual BasicDeliverEventArgs GetDeliveryArgs(IPipeContext context)
		{
			var args = DeliveryArgsFunc(context);
			if (args == null)
			{
				Logger.LogWarning("Delivery args not found in Pipe context.");
			}
			return args;
		}

		protected virtual PublicationAddress GetReplyTo(BasicDeliverEventArgs args)
		{
			var replyTo = ReplyToFunc(args);
			if (replyTo == null)
			{
				Logger.LogWarning("Reply to address not found in Pipe context.");
			}
			else
			{
				args.BasicProperties.ReplyTo = replyTo.RoutingKey;
				Logger.LogInformation("Using reply address with exchange {exchangeName} and routing key '{routingKey}'", replyTo.ExchangeName, replyTo.RoutingKey);
			}
			return replyTo;
		}

		protected virtual void SaveInContext(IPipeContext context, PublicationAddress replyTo)
		{
			if (ContextSaveAction == null)
			{
				Logger.LogWarning("No context save action found. Reply to address will not be saved.");
			}
			ContextSaveAction?.Invoke(context, replyTo);
		}
	}
}
