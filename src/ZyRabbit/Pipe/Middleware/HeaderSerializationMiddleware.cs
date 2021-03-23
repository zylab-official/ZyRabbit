using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using ZyRabbit.Common;
using ZyRabbit.Serialization;

namespace ZyRabbit.Pipe.Middleware
{
	public class HeaderSerializationOptions
	{
		public Predicate<IPipeContext> ExecutePredicate { get; set; }
		public Func<IPipeContext, IBasicProperties> BasicPropsFunc { get; set; }
		public Func<IPipeContext, object> RetrieveItemFunc { get; set; }
		public Func<IPipeContext, object> CreateItemFunc { get; set; }
		public Func<IPipeContext, string> HeaderKeyFunc { get; set; }
	}

	public class HeaderSerializationMiddleware : StagedMiddleware
	{
		protected readonly ISerializer Serializer;
		protected readonly Func<IPipeContext, IBasicProperties> BasicPropsFunc;
		protected readonly Func<IPipeContext, object> RetrieveItemFunc;
		protected readonly Func<IPipeContext, object> CreateItemFunc;
		protected readonly Predicate<IPipeContext> ExecutePredicate;
		protected readonly Func<IPipeContext, string> HeaderKeyFunc;

		public HeaderSerializationMiddleware(ISerializer serializer, HeaderSerializationOptions options = null)
		{
			Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
			ExecutePredicate = options?.ExecutePredicate ?? (context => true);
			BasicPropsFunc = options?.BasicPropsFunc ?? (context => context.GetBasicProperties());
			RetrieveItemFunc = options?.RetrieveItemFunc ?? (context => null);
			CreateItemFunc = options?.CreateItemFunc ?? (context => null);
			CreateItemFunc = options?.CreateItemFunc ?? (context => null);
			HeaderKeyFunc = options?.HeaderKeyFunc ?? (context => null);
		}

		public override async Task InvokeAsync(IPipeContext context, CancellationToken token = default(CancellationToken))
		{
			if (!ShouldExecute(context))
			{
				await Next.InvokeAsync(context, token);
				return;
			}
			var properties = GetBasicProperties(context);
			var headerKey = GetHeaderKey(context);
			if (properties.Headers.ContainsKey(headerKey))
			{
				await Next.InvokeAsync(context, token);
				return;
			}

			var item = GetHeaderItem(context) ?? CreateHeaderItem(context);
			var serializedItem = SerializeItem(item, context);
			properties.Headers.TryAdd(headerKey, serializedItem);

			await Next.InvokeAsync(context, token);
		}

		protected virtual bool ShouldExecute(IPipeContext context)
		{
			return ExecutePredicate.Invoke(context);
		}

		protected virtual IBasicProperties GetBasicProperties(IPipeContext context)
		{
			return BasicPropsFunc?.Invoke(context);
		}

		protected virtual object GetHeaderItem(IPipeContext context)
		{
			return RetrieveItemFunc?.Invoke(context);
		}

		protected virtual object CreateHeaderItem(IPipeContext context)
		{
			return CreateItemFunc?.Invoke(context);
		}

		protected virtual byte[] SerializeItem(object item, IPipeContext context)
		{
			return Serializer.Serialize(item);
		}

		protected virtual string GetHeaderKey(IPipeContext context)
		{
			return HeaderKeyFunc?.Invoke(context);
		}

		public override string StageMarker => Pipe.StageMarker.BasicPropertiesCreated;
	}
}
