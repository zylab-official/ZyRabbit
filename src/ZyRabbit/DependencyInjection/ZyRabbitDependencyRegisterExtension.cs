using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using System.Linq;
using ZyRabbit.Channel;
using ZyRabbit.Channel.Abstraction;
using ZyRabbit.Common;
using ZyRabbit.Configuration;
using ZyRabbit.Configuration.BasicPublish;
using ZyRabbit.Configuration.Consume;
using ZyRabbit.Configuration.Consumer;
using ZyRabbit.Configuration.Exchange;
using ZyRabbit.Configuration.Publisher;
using ZyRabbit.Configuration.Queue;
using ZyRabbit.Consumer;
using ZyRabbit.Instantiation;
using ZyRabbit.Pipe;
using ZyRabbit.Serialization;
using ZyRabbit.Subscription;

namespace ZyRabbit.DependencyInjection
{
	public static class ZyRabbitDependencyRegisterExtension
	{
		public static IDependencyRegister AddZyRabbit(this IDependencyRegister register, ZyRabbitOptions options = null)
		{
			register
				.AddSingleton(options?.ClientConfiguration ?? ZyRabbitConfiguration.Local)
				.AddSingleton<IConnectionFactory, ConnectionFactory>(provider =>
				{
					var cfg = provider.GetService<ZyRabbitConfiguration>();
					return new ConnectionFactory
					{
						VirtualHost = cfg.VirtualHost,
						UserName = cfg.Username,
						Password = cfg.Password,
						Port = cfg.Port,
						HostName = cfg.Hostnames.FirstOrDefault() ?? string.Empty,
						AutomaticRecoveryEnabled = cfg.AutomaticRecovery,
						TopologyRecoveryEnabled = cfg.TopologyRecovery,
						NetworkRecoveryInterval = cfg.RecoveryInterval,
						ClientProperties = provider.GetService<IClientPropertyProvider>().GetClientProperties(cfg),
						Ssl = cfg.Ssl
					};
				})
				.AddSingleton<IChannelPoolFactory, AutoScalingChannelPoolFactory>()
				.AddSingleton(resolver => AutoScalingOptions.Default)
				.AddSingleton<IClientPropertyProvider, ClientPropertyProvider>()
				.AddSingleton<ISerializer>(resolver => new Serialization.JsonSerializer(new Newtonsoft.Json.JsonSerializer
				{
					TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
					Formatting = Formatting.None,
					CheckAdditionalContent = true,
					ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
					ObjectCreationHandling = ObjectCreationHandling.Auto,
					DefaultValueHandling = DefaultValueHandling.Ignore,
					TypeNameHandling = TypeNameHandling.Auto,
					ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
					MissingMemberHandling = MissingMemberHandling.Ignore,
					PreserveReferencesHandling = PreserveReferencesHandling.Objects,
					NullValueHandling = NullValueHandling.Ignore
				}))
				.AddSingleton<IConsumerFactory, ConsumerFactory>()
				.AddSingleton<IChannelFactory>(resolver =>
				{
					var channelFactory = new ChannelFactory(
						resolver.GetService<IConnectionFactory>(),
						resolver.GetService<ZyRabbitConfiguration>(),
						resolver.GetService<ILogger<ChannelFactory>>());
					channelFactory
						.ConnectAsync()
						.ConfigureAwait(false)
						.GetAwaiter()
						.GetResult();
					return channelFactory;
				})
				.AddSingleton<ISubscriptionRepository, SubscriptionRepository>()
				.AddSingleton<ITopologyProvider, TopologyProvider>()
				.AddTransient<IPublisherConfigurationFactory, PublisherConfigurationFactory>()
				.AddTransient<IBasicPublishConfigurationFactory, BasicPublishConfigurationFactory>()
				.AddTransient<IConsumerConfigurationFactory, ConsumerConfigurationFactory>()
				.AddTransient<IConsumeConfigurationFactory, ConsumeConfigurationFactory>()
				.AddTransient<IExchangeDeclarationFactory, ExchangeDeclarationFactory>()
				.AddTransient<IQueueConfigurationFactory, QueueDeclarationFactory>()
				.AddSingleton<INamingConventions, NamingConventions>()
				.AddSingleton<IExclusiveLock, ExclusiveLock>()
				.AddSingleton<IBusClient, BusClient>()
				.AddSingleton<IResourceDisposer, ResourceDisposer>()
				.AddTransient<IInstanceFactory>(resolver => new InstanceFactory(resolver))
				.AddSingleton<IPipeContextFactory, PipeContextFactory>()
				.AddTransient<IExtendedPipeBuilder, PipeBuilder>(resolver => new PipeBuilder(resolver))
				.AddSingleton<IPipeBuilderFactory>(provider => new PipeBuilderFactory(provider));

			var clientBuilder = new ClientBuilder();
			options?.Plugins?.Invoke(clientBuilder);
			clientBuilder.DependencyInjection?.Invoke(register);
			register.AddSingleton(clientBuilder.PipeBuilderAction);
			options?.DependencyInjection?.Invoke(register);

			if (!register.IsRegistered(typeof(ILogger<>)))
			{
				register.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
			}

			if (!register.IsRegistered(typeof(ILogger)))
			{
				register.AddSingleton<ILogger, NullLogger>(resolver => NullLogger.Instance);
			}

			if (!register.IsRegistered(typeof(ILoggerFactory)))
			{
				register.AddSingleton<ILoggerFactory, NullLoggerFactory>();
			}

			return register;
		}
	}
}
