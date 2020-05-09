using System;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Configuration.Consumer;
using ZyRabbit.Configuration.Publisher;
using ZyRabbit.Operations.Request.Configuration;
using ZyRabbit.Operations.Request.Configuration.Abstraction;
using ZyRabbit.Operations.Request.Core;
using ZyRabbit.Pipe;

namespace ZyRabbit.Operations.Request.Middleware
{
	public class RequestConfigurationMiddleware : Pipe.Middleware.Middleware
	{
		private readonly IRequestConfigurationFactory _factory;

		public RequestConfigurationMiddleware(IPublisherConfigurationFactory publisher, IConsumerConfigurationFactory consumer)
		{
			_factory = new RequestConfigurationFactory(publisher, consumer);
		}

		public RequestConfigurationMiddleware(IRequestConfigurationFactory factory)
		{
			_factory = factory;
		}

		public override Task InvokeAsync(IPipeContext context, CancellationToken token)
		{
			var requestType = context.GetRequestMessageType();
			var responseType = context.GetResponseMessageType();

			if (requestType == null)
				throw new ArgumentNullException(nameof(requestType));
			if (responseType == null)
				throw new ArgumentNullException(nameof(responseType));

			var defaultCfg = _factory.Create(requestType, responseType);

			var builder = new RequestConfigurationBuilder(defaultCfg);
			var action = context.Get<Action<IRequestConfigurationBuilder>>(PipeKey.ConfigurationAction);
			action?.Invoke(builder);
			var requestConfig = builder.Config;

			context.Properties.TryAdd(RequestKey.Configuration, requestConfig);
			context.Properties.TryAdd(PipeKey.PublisherConfiguration, requestConfig.Request);
			context.Properties.TryAdd(PipeKey.ConsumerConfiguration, requestConfig.Response);
			context.Properties.TryAdd(PipeKey.ConsumeConfiguration, requestConfig.Response.Consume);
			return Next.InvokeAsync(context, token);
		}
	}
}
