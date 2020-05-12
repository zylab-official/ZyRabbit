using System;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Configuration.Get;
using ZyRabbit.Operations.Get;
using ZyRabbit.Operations.Get.Middleware;
using ZyRabbit.Operations.Get.Model;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit
{
	public static class GetOfTOperation
	{

		public static readonly Action<IPipeBuilder> DeserializedBodyGetPipe = pipe => pipe
			.Use<GetConfigurationMiddleware>()
			.Use<ConventionNamingMiddleware>()
			.Use<ChannelCreationMiddleware>()
			.Use<BasicGetMiddleware>()
			.Use<BodyDeserializationMiddleware>(new MessageDeserializationOptions
			{
				BodyTypeFunc = context => context.GetMessageType(),
				BodyFunc = context => context.GetBasicGetResult()?.Body
			})
			.Use<AckableResultMiddleware>(new AckableResultOptions
			{
				DeliveryTagFunc = context => context.GetBasicGetResult()?.DeliveryTag ?? 0,
				ContentFunc = context => context.GetMessage()
			});

		public static Task<Ackable<TMessage>> GetAsync<TMessage>(this IBusClient busClient, Action<IGetConfigurationBuilder> config = null, CancellationToken token = default(CancellationToken))
		{
			return GetAsync<TMessage>(busClient, config, null, token);
		}

		internal static async Task<Ackable<TMessage>> GetAsync<TMessage>(this IBusClient busClient, Action<IGetConfigurationBuilder> config = null, Action<IPipeContext> pipeAction = null, CancellationToken token = default(CancellationToken))
		{
			var result = await busClient
				.InvokeAsync(DeserializedBodyGetPipe, context =>
				{
					context.Properties.Add(PipeKey.ConfigurationAction, config);
					context.Properties.Add(PipeKey.MessageType, typeof(TMessage));
					pipeAction?.Invoke(context);
				}, token);
			return result.Get<Ackable<object>>(GetKey.AckableResult).AsAckable<TMessage>();
		}
	}
}
