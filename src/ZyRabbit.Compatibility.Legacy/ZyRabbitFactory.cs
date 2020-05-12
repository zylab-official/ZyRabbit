using System;
using ZyRabbit.Compatibility.Legacy.Configuration;
using ZyRabbit.DependencyInjection;
using ZyRabbit.Enrichers.MessageContext;
using ZyRabbit.Enrichers.MessageContext.Context;
using ZyRabbit.Instantiation;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;
using ZyRabbitConfiguration = ZyRabbit.Configuration.ZyRabbitConfiguration;

namespace ZyRabbit.Compatibility.Legacy
{
	public class ZyRabbitFactory
	{
		public static IBusClient<TMessageContext> CreateClient<TMessageContext>(ZyRabbitOptions options = null)
			where TMessageContext : IMessageContext
		{
			options = options ?? new ZyRabbitOptions();
			options.DependencyInjection = options.DependencyInjection ?? (register => { });
			options.DependencyInjection += register => register.AddSingleton<IConfigurationEvaluator, ConfigurationEvaluator>();
			options.ClientConfiguration = options?.ClientConfiguration ?? ZyRabbitConfiguration.Local;
			options.Plugins = options.Plugins ?? (builder => { });
			options.Plugins += builder => builder
				.UseMessageContext(context => new MessageContext { GlobalRequestId = Guid.NewGuid() })
				.UseContextForwarding();
			var simpleIoc = new SimpleDependencyInjection();
			var client = Instantiation.ZyRabbitFactory.CreateSingleton(options, simpleIoc, ioc => simpleIoc);
			return new BusClient<TMessageContext>(client, simpleIoc.GetService<IConfigurationEvaluator>());
		}

		public static IBusClient CreateClient(ZyRabbitOptions options = null)
		{
			options = options ?? new ZyRabbitOptions();
			options.DependencyInjection = options.DependencyInjection ?? (register => { });
			options.DependencyInjection += register => register.AddSingleton<IConfigurationEvaluator, ConfigurationEvaluator>();
			options.ClientConfiguration = options?.ClientConfiguration ?? ZyRabbitConfiguration.Local;
			options.Plugins = options.Plugins ?? (builder => { });
			options.Plugins += builder => builder
				.UseMessageContext(context => new MessageContext {GlobalRequestId = Guid.NewGuid()})
				.UseContextForwarding();
			var simpleIoc = new SimpleDependencyInjection();
			var client = Instantiation.ZyRabbitFactory.CreateSingleton(options, simpleIoc, ioc => simpleIoc);
			return new BusClient(client, simpleIoc.GetService<IConfigurationEvaluator>());
		}
	}
}
