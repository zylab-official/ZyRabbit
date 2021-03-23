using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using ZyRabbit.Common;
using ZyRabbit.Exceptions;
using ZyRabbit.Operations.Request.Core;
using ZyRabbit.Pipe;
using ZyRabbit.Serialization;

namespace ZyRabbit.Operations.Request.Middleware
{
	public class ResponderExceptionOptions
	{
		public Func<IPipeContext, object> MessageFunc { get; set; }
		public Func<ExceptionInformation, IPipeContext, Task> HandlerFunc { get; set; }
		public Func<IPipeContext, Type> ResponseTypeFunc { get; set; }
		public Func<IPipeContext, BasicDeliverEventArgs> DeliveryArgsFunc { get; set; }
	}

	public class ResponderExceptionMiddleware : Pipe.Middleware.Middleware
	{
		private readonly ISerializer _serializer;
		protected readonly Func<IPipeContext, object> ExceptionInfoFunc;
		protected readonly Func<ExceptionInformation, IPipeContext, Task> HandlerFunc;
		protected readonly ILogger<ResponderExceptionMiddleware> Logger;
		protected readonly Func<IPipeContext, Type> ResponseTypeFunc;
		private readonly Func<IPipeContext, BasicDeliverEventArgs> _deliveryArgFunc;

		public ResponderExceptionMiddleware(ISerializer serializer, ILogger<ResponderExceptionMiddleware> logger, ResponderExceptionOptions options = null)
		{
			_serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			ExceptionInfoFunc = options?.MessageFunc ?? (context => context.GetResponseMessage());
			HandlerFunc = options?.HandlerFunc;
			_deliveryArgFunc = options?.DeliveryArgsFunc ?? (context => context.GetDeliveryEventArgs());
			ResponseTypeFunc = options?.ResponseTypeFunc ?? (context =>
			{
				var type = GetDeliverEventArgs(context)?.BasicProperties.Type;
				return !string.IsNullOrWhiteSpace(type) ? Type.GetType(type, false) : typeof(object);
			});
		}

		public override Task InvokeAsync(IPipeContext context, CancellationToken token = new CancellationToken())
		{
			var responseType = GetResponseType(context);
			if (responseType == typeof(ExceptionInformation))
			{
				var exceptionInfo = GetExceptionInfo(context);
				return HandleRespondException(exceptionInfo, context);
			}
			return Next.InvokeAsync(context, token);
		}

		protected virtual BasicDeliverEventArgs GetDeliverEventArgs(IPipeContext context)
		{
			return _deliveryArgFunc?.Invoke(context);
		}

		protected virtual Type GetResponseType(IPipeContext context)
		{
			return ResponseTypeFunc?.Invoke(context);
		}

		protected virtual byte[] GetMessageBody(IPipeContext context)
		{
			var deliveryArgs = GetDeliverEventArgs(context);
			return deliveryArgs?.Body ?? new byte[0];
		}

		protected virtual ExceptionInformation GetExceptionInfo(IPipeContext context)
		{
			var body = GetMessageBody(context);
			try
			{
				return _serializer.Deserialize<ExceptionInformation>(body);
			}
			catch (Exception e)
			{
				return new ExceptionInformation
				{
					Message =
						$"An unhandled exception was thrown by the responder, but the requesting client was unable to deserialize exception info. {Encoding.UTF8.GetString(body)}.",
					InnerMessage = e.Message,
					StackTrace = e.StackTrace,
					ExceptionType = e.GetType().Name
				};
			}
		}

		protected virtual Task HandleRespondException(ExceptionInformation exceptionInfo, IPipeContext context)
		{
			Logger.LogInformation("An unhandled exception occured when remote tried to handle request.\n  Message: {exceptionMessage}\n  Stack Trace: {stackTrace}", exceptionInfo.Message, exceptionInfo.StackTrace);

			if (HandlerFunc != null)
			{
				return HandlerFunc(exceptionInfo, context);
			}

			var exception = new MessageHandlerException(exceptionInfo.Message)
			{
				InnerExceptionType = exceptionInfo.ExceptionType,
				InnerStackTrace = exceptionInfo.StackTrace,
				InnerMessage = exceptionInfo.InnerMessage
			};
			return TaskUtil.FromException(exception);
		}
	}
}
