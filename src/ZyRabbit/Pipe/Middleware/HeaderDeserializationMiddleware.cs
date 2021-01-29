using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using ZyRabbit.Serialization;

namespace ZyRabbit.Pipe.Middleware
{
	public class HeaderDeserializationOptions
	{
		public Func<IPipeContext, BasicDeliverEventArgs> DeliveryArgsFunc { get; set; }
		public Func<IPipeContext, string> HeaderKeyFunc { get; set; }
		public Func<IPipeContext, Type> HeaderTypeFunc { get; set; }
		public Action<IPipeContext, object> ContextSaveAction { get; set; }
	}

	public class HeaderDeserializationMiddleware : StagedMiddleware
	{
		protected readonly ISerializer Serializer;
		protected readonly Func<IPipeContext, BasicDeliverEventArgs> DeliveryArgsFunc;
		protected readonly Action<IPipeContext, object> ContextSaveAction;
		protected readonly Func<IPipeContext, string> HeaderKeyFunc;
		protected readonly Func<IPipeContext, Type> HeaderTypeFunc;
		protected readonly ILogger<HeaderDeserializationMiddleware> Logger;

		public HeaderDeserializationMiddleware(ISerializer serializer, ILogger<HeaderDeserializationMiddleware> logger, HeaderDeserializationOptions options = null)
		{
			DeliveryArgsFunc = options?.DeliveryArgsFunc ?? (context => context.GetDeliveryEventArgs());
			HeaderKeyFunc = options?.HeaderKeyFunc;
			ContextSaveAction = options?.ContextSaveAction ?? ((context, item) => context.Properties.TryAdd(HeaderKeyFunc(context), item));
			HeaderTypeFunc = options?.HeaderTypeFunc ?? (context =>typeof(object)) ;
			Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public override async Task InvokeAsync(IPipeContext context, CancellationToken token = default(CancellationToken))
		{
			var headerObject = GetHeaderObject(context);
			if (headerObject != null)
			{
				SaveInContext(context, headerObject);
			}
			await Next.InvokeAsync(context, token);
		}

		protected virtual void SaveInContext(IPipeContext context, object headerValue)
		{
			ContextSaveAction?.Invoke(context, headerValue);
		}

		protected virtual object GetHeaderObject(IPipeContext context)
		{
			var bytes = GetHeaderBytes(context);
			if (bytes == null)
			{
				return null;
			}
			var type = GetHeaderType(context);
			return Serializer.Deserialize(type, bytes);
		}

		protected virtual byte[] GetHeaderBytes(IPipeContext context)
		{
			var headerKey = GetHeaderKey(context);
			var args = GetDeliveryArgs(context);
			if (string.IsNullOrEmpty(headerKey))
			{
				Logger.LogDebug("Key {headerKey} not found.", headerKey);
				return null;
			}
			if (args == null)
			{
				Logger.LogDebug("DeliveryEventArgs not found.");
				return null;
			}
			if (args.BasicProperties.Headers == null || !args.BasicProperties.Headers.ContainsKey(headerKey))
			{
				Logger.LogDebug("BasicProperties Header does not contain {headerKey}", headerKey);
				return null;
			}

			object headerBytes;
			return args.BasicProperties.Headers.TryGetValue(headerKey, out headerBytes)
				? headerBytes as byte[]
				: null;
		}

		protected virtual BasicDeliverEventArgs GetDeliveryArgs(IPipeContext context)
		{
			var args = DeliveryArgsFunc(context);
			return args;
		}

		protected virtual string GetHeaderKey(IPipeContext context)
		{
			var key = HeaderKeyFunc(context);
			return key;
		}

		protected virtual Type GetHeaderType(IPipeContext context)
		{
			var type = HeaderTypeFunc(context);
			return type;
		}

		public override string StageMarker => Pipe.StageMarker.MessageReceived;
	}
}
