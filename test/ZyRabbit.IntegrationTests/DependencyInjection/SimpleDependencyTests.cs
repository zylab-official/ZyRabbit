using System;
using System.Threading.Tasks;
using ZyRabbit.Configuration;
using ZyRabbit.Instantiation;
using Xunit;
using ZyRabbit.DependencyInjection;
using ZyRabbit.Common;
using ZyRabbit.IntegrationTests.TestMessages;
using RabbitMQ.Client.Exceptions;
using Microsoft.Extensions.Logging;

namespace ZyRabbit.IntegrationTests.DependencyInjection
{
	public class SimpleDependencyTests
	{
		[Fact]
		public async Task Should_Be_Able_To_Publish_Message_From_Resolved_Client()
		{
			/* Setup */
			using var container = new SimpleDependencyInjection();
			container.AddZyRabbit();

			/* Test */
			var client = container.GetService<IBusClient>();
			await client.PublishAsync(new BasicMessage());
			await client.DeleteExchangeAsync<BasicMessage>();
			var disposer = container.GetService<IResourceDisposer>();

			/* Assert */
			disposer.Dispose();
		}

		[Fact]
		public async Task Should_Honor_Client_Config_From_Options()
		{
			var config = ZyRabbitConfiguration.Local;
			config.VirtualHost = "/foo";
			await Assert.ThrowsAnyAsync<BrokerUnreachableException>(async () =>
			{
				var factory = ZyRabbitFactory.CreateTestInstanceFactory(new ZyRabbitOptions {ClientConfiguration = config});
				var client = factory.Create();
				await client.CreateChannelAsync();
			});
		}

		[Fact]
		public void Should_Be_Able_To_Resolve_Client_With_Plugins_From_SimpleDependency()
		{
			/* Setup */
			using var container = new SimpleDependencyInjection();
			container.AddZyRabbit(new ZyRabbitOptions
			{
				Plugins = p => p.UseStateMachine()
			});

			/* Test */
			var client = container.GetService<IBusClient>();
			var disposer = container.GetService<IResourceDisposer>();

			/* Assert */
			disposer.Dispose();
		}

		[Fact]
		public void Should_Be_Able_To_Resolve_Logger()
		{
			/* Setup */
			using var container = new SimpleDependencyInjection();
			container.AddZyRabbit();

			/* Test */
			var logger1 = container.GetService<ILogger<IExclusiveLock>>();
			var logger2 = container.GetService<ILogger<IExclusiveLock>>();
			Assert.Same(logger1, logger2);
		}

		[Fact]
		public void Should_Be_Able_To_Check_Logger()
		{
			/* Setup */
			using var container = new SimpleDependencyInjection();
			container.AddZyRabbit();

			/* Test */
			Assert.True(container.IsRegistered(typeof(ILogger<>)));
		}
	}
}
