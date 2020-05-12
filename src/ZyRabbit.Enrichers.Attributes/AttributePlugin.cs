using System;
using ZyRabbit.Enrichers.Attributes.Middleware;
using ZyRabbit.Instantiation;
using ZyRabbit.Pipe;

namespace ZyRabbit
{
	public static class AttributePlugin
	{
		private const string IncommingMessageType = "IncommingMessageType";
		private const string OutgoingMessageType = "OutgoingMessageType";

		public static IClientBuilder UseAttributeRouting(this IClientBuilder builder, ConsumeAttributeOptions consume = null, ProduceAttributeOptions produce = null)
		{
			if (consume == null)
			{
				consume = new ConsumeAttributeOptions
				{
					MessageTypeFunc = context => context.Get<Type>(IncommingMessageType) ?? context.Get<Type>(PipeKey.MessageType)
				};
			}
			if (produce == null)
			{
				produce = new ProduceAttributeOptions
				{
					MessageTypeFunc = context => context.Get<Type>(OutgoingMessageType) ?? context.Get<Type>(PipeKey.MessageType)
				};
			}
			builder.Register(pipe => pipe
				.Use<ConsumeAttributeMiddleware>(consume)
				.Use<ProduceAttributeMiddleware>(produce));
			return builder;
		}
	}
}
