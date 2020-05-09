using System;
using ZyRabbit.Configuration;
using ZyRabbit.Instantiation;

namespace ZyRabbit.IntegrationTests
{
	public static class ZyRabbitFactory
	{
		public static Instantiation.Disposable.BusClient CreateTestClient(Action<IClientBuilder> plugins)
		{
			return CreateTestClient(new ZyRabbitOptions {Plugins = plugins});
		}

		public static Instantiation.Disposable.BusClient CreateTestClient(ZyRabbitOptions options = null)
		{
			return Instantiation.ZyRabbitFactory.CreateSingleton(GetTestOptions(options));
		}

		public static InstanceFactory CreateTestInstanceFactory(ZyRabbitOptions options = null)
		{
			return Instantiation.ZyRabbitFactory.CreateInstanceFactory(GetTestOptions(options));
		}

		private static ZyRabbitOptions GetTestOptions(ZyRabbitOptions options)
		{
			options = options ?? new ZyRabbitOptions();
			options.ClientConfiguration = options.ClientConfiguration ?? ZyRabbitConfiguration.Local;
			options.ClientConfiguration.Queue.AutoDelete = true;
			options.ClientConfiguration.Exchange.AutoDelete = true;

			return options;
		}
	}
}
