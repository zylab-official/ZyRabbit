﻿using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Configuration.Publisher;
using ZyRabbit.Pipe;

namespace ZyRabbit.Operations.Publish.Middleware
{
	public class PublishConfigurationOptions
	{
		public Func<IPipeContext, string> ExchangeFunc { get; set; }
		public Func<IPipeContext, string> RoutingKeyFunc { get; set; }
		public Func<IPipeContext, Type> MessageTypeFunc { get; set; }
	}

	public class PublishConfigurationMiddleware : Pipe.Middleware.Middleware
	{
		protected readonly IPublisherConfigurationFactory PublisherFactory;
		protected readonly Func<IPipeContext, string> ExchangeFunc;
		protected readonly Func<IPipeContext, string> RoutingKeyFunc;
		protected readonly Func<IPipeContext, Type> MessageTypeFunc;
		protected readonly ILogger<PublishConfigurationMiddleware> Logger;

		public PublishConfigurationMiddleware(IPublisherConfigurationFactory publisherFactory, ILogger<PublishConfigurationMiddleware> logger, PublishConfigurationOptions options = null)
		{
			PublisherFactory = publisherFactory ?? throw new ArgumentNullException(nameof(publisherFactory));
			ExchangeFunc = options?.ExchangeFunc ?? (context => context.GetPublishConfiguration()?.Exchange.Name);
			RoutingKeyFunc = options?.RoutingKeyFunc ?? (context => context.GetPublishConfiguration()?.RoutingKey);
			MessageTypeFunc = options?.MessageTypeFunc ?? (context => context.GetMessageType());
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public override Task InvokeAsync(IPipeContext context, CancellationToken token)
		{
			var config = ExtractConfigFromMessageType(context) ?? ExtractConfigFromStrings(context);
			if (config == null)
			{
				Logger.LogWarning("Unable to find PublisherConfiguration from message type or parameters.");
				throw new ArgumentNullException(nameof(config));
			}

			var action = context.Get<Action<IPublisherConfigurationBuilder>>(PipeKey.ConfigurationAction);
			if (action != null)
			{
				Logger.LogDebug($"Custom configuration supplied. Applying.");
				var builder = new PublisherConfigurationBuilder(config);
				action(builder);
				config = builder.Config;
			}

			context.Properties.TryAdd(PipeKey.PublisherConfiguration, config);
			context.Properties.TryAdd(PipeKey.BasicPublishConfiguration, config);
			context.Properties.TryAdd(PipeKey.ExchangeDeclaration, config.Exchange);
			context.Properties.TryAdd(PipeKey.BasicProperties, config.BasicProperties);
			context.Properties.TryAdd(PipeKey.ReturnCallback, config.ReturnCallback);

			return Next.InvokeAsync(context, token);
		}

		protected virtual Type GetMessageType(IPipeContext context)
		{
			return MessageTypeFunc(context);
		}

		protected virtual PublisherConfiguration ExtractConfigFromStrings(IPipeContext context)
		{
			var routingKey = RoutingKeyFunc(context);
			var exchange = ExchangeFunc(context);
			return PublisherFactory.Create(exchange, routingKey);
		}

		protected virtual PublisherConfiguration ExtractConfigFromMessageType(IPipeContext context)
		{
			var messageType = GetMessageType(context);
			return messageType == null
				? null
				: PublisherFactory.Create(messageType);
		}
	}
}
