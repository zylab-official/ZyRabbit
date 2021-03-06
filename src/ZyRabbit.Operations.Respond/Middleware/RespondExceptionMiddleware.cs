﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ZyRabbit.Common;
using ZyRabbit.Configuration.Consume;
using ZyRabbit.Exceptions;
using ZyRabbit.Operations.Respond.Core;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit.Operations.Respond.Middleware
{
	public class RespondExceptionOptions
	{
		public Func<IPipeContext, BasicDeliverEventArgs> DeliveryArgsFunc { get; set; }
		public Func<IPipeContext, ConsumeConfiguration> ConsumeConfigFunc { get; set; }
		public Action<IPipeContext, ExceptionInformation> SaveAction { get; set; }
		public Action<IPipeBuilder> InnerPipe { get; set; }
	}

	public class RespondExceptionMiddleware : ExceptionHandlingMiddleware
	{
		protected readonly Func<IPipeContext, BasicDeliverEventArgs> DeliveryArgsFunc;
		protected readonly Func<IPipeContext, ConsumeConfiguration> ConsumeConfigFunc;
		protected readonly Func<IPipeContext, IModel> ChannelFunc;
		protected readonly Action<IPipeContext, ExceptionInformation> SaveAction;

		public RespondExceptionMiddleware(IPipeBuilderFactory factory, ILogger<ExceptionHandlingMiddleware> logger, RespondExceptionOptions options = null)
			: base(factory, logger, new ExceptionHandlingOptions { InnerPipe = options?.InnerPipe })
		{
			DeliveryArgsFunc = options?.DeliveryArgsFunc ?? (context => context.GetDeliveryEventArgs());
			ConsumeConfigFunc = options?.ConsumeConfigFunc ?? (context => context.GetConsumeConfiguration());
			SaveAction = options?.SaveAction ?? ((context, information) => context.Properties.TryAdd(RespondKey.ResponseMessage, information));
		}

		protected override Task OnExceptionAsync(Exception exception, IPipeContext context, CancellationToken token)
		{
			var innerException = UnwrapInnerException(exception);
			var args = GetDeliveryArgs(context);
			var cfg = GetConsumeConfiguration(context);
			AddAcknowledgementToContext(context, cfg);
			var exceptionInfo = CreateExceptionInformation(innerException, args, cfg, context);
			SaveInContext(context, exceptionInfo);
			return Next.InvokeAsync(context, token);
		}

		protected virtual void AddAcknowledgementToContext(IPipeContext context, ConsumeConfiguration cfg)
		{
			if (cfg.AutoAck)
			{
				return;
			}
			context.Properties.TryAdd(PipeKey.MessageAcknowledgement, new Ack());
		}

		protected virtual BasicDeliverEventArgs GetDeliveryArgs(IPipeContext context)
		{
			return DeliveryArgsFunc(context);
		}

		protected virtual ConsumeConfiguration GetConsumeConfiguration(IPipeContext context)
		{
			return ConsumeConfigFunc(context);
		}

		protected virtual ExceptionInformation CreateExceptionInformation(Exception exception, BasicDeliverEventArgs args, ConsumeConfiguration cfg, IPipeContext context)
		{
			return new ExceptionInformation
			{
				Message = $"An unhandled exception was thrown when consuming a message\n  MessageId: {args.BasicProperties.MessageId}\n  Queue: '{cfg.QueueName}'\n  Exchange: '{cfg.ExchangeName}'\nSee inner exception for more details.",
				ExceptionType = exception.GetType().FullName,
				StackTrace = exception.StackTrace,
				InnerMessage = exception.Message
			};
		}

		protected virtual void SaveInContext(IPipeContext context, ExceptionInformation info)
		{
			SaveAction?.Invoke(context, info);
		}
	}
}
