using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Exceptions;
using System.Threading.Tasks;
using Xunit;
using ZyRabbit.Common;
using ZyRabbit.Configuration;
using ZyRabbit.DependencyInjection.ServiceCollection;
using ZyRabbit.IntegrationTests.TestMessages;
using ZyRabbit.Instantiation;
using ZyRabbit.Operations.StateMachine.Middleware;
using ZyRabbit.Operations.StateMachine.Core;
using System.Linq;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit.IntegrationTests.DependencyInjection
{
	public class ServiceCollectionDependencyTests
	{
		[Fact]
		public async Task Should_Be_Able_To_Publish_Message_From_Resolved_Client()
		{
			/* Setup */
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddZyRabbit();
			using var provider = serviceCollection.BuildServiceProvider();

			/* Test */
			var client = provider.GetService<IBusClient>();
			await client.PublishAsync(new BasicMessage());
			await client.DeleteExchangeAsync<BasicMessage>();
			var disposer = provider.GetService<IResourceDisposer>();

			/* Assert */
			disposer.Dispose();
		}

		[Fact]
		public async Task Should_Honor_Client_Config_From_Options()
		{
			/* Setup */
			var serviceCollection = new ServiceCollection();
			var config = ZyRabbitConfiguration.Local;
			config.VirtualHost = "/foo";
			serviceCollection.AddZyRabbit(new ZyRabbitOptions
			{
				ClientConfiguration = config
			});

			/* Test */
			await Assert.ThrowsAnyAsync<BrokerUnreachableException>(async () =>
			{
				using var provider = serviceCollection.BuildServiceProvider();
				var client = provider.GetService<IBusClient>();
				await client.CreateChannelAsync();
			});
		}

		[Fact]
		public void Should_Be_Able_To_Resolve_Client_With_Plugins_From_ServiceCollection()
		{
			/* Setup */
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddZyRabbit(new ZyRabbitOptions
			{
				Plugins = p => p.UseStateMachine()
			});
			using var provider = serviceCollection.BuildServiceProvider();
			var providerAdapter = new ServiceProviderAdapter(provider);

			/* Test */
			var client = providerAdapter.GetService<IBusClient>();
			var middleware = providerAdapter.GetService<RetrieveStateMachineMiddleware>();

			/* Assert */
			Assert.NotNull(client);
			Assert.NotNull(middleware);
		}

		[Fact]
		public void Should_Be_Able_To_Resolve_Logger()
		{
			/* Setup */
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddZyRabbit();
			using var provider = serviceCollection.BuildServiceProvider();

			/* Test */
			var logger1 = provider.GetService<ILogger<IExclusiveLock>>();
			var logger2 = provider.GetService<ILogger<IExclusiveLock>>();
			Assert.Same(logger1, logger2);
			Assert.NotNull(logger1);
		}
	}
}
