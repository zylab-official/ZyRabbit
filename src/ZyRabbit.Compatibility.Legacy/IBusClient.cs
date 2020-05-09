using System;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Common;
using ZyRabbit.Compatibility.Legacy.Configuration.Publish;
using ZyRabbit.Compatibility.Legacy.Configuration.Request;
using ZyRabbit.Compatibility.Legacy.Configuration.Respond;
using ZyRabbit.Compatibility.Legacy.Configuration.Subscribe;
using ZyRabbit.Enrichers.MessageContext.Context;
using ZyRabbit.Subscription;

namespace ZyRabbit.Compatibility.Legacy
{
	public interface IBusClient<out TMessageContext> where TMessageContext : IMessageContext
	{
		ISubscription SubscribeAsync<T>(Func<T, TMessageContext, Task> subscribeMethod, Action<ISubscriptionConfigurationBuilder> configuration = null);

		Task PublishAsync<T>(T message = default(T), Guid globalMessageId = new Guid(), Action<IPublishConfigurationBuilder> configuration = null);

		ISubscription RespondAsync<TRequest, TResponse>(Func<TRequest, TMessageContext, Task<TResponse>> onMessage, Action<IResponderConfigurationBuilder> configuration = null);

		Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest message = default(TRequest), Guid globalMessageId = new Guid(), Action<IRequestConfigurationBuilder> configuration = null);
	}

	public interface IBusClient : IBusClient<MessageContext> { }
}
