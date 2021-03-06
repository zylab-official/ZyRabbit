using System.Threading.Tasks;
using ZyRabbit.Configuration;
using ZyRabbit.Instantiation;
using Xunit;
using ZyRabbit.DependencyInjection;
using ZyRabbit.Common;
using ZyRabbit.IntegrationTests.TestMessages;
using RabbitMQ.Client.Exceptions;
using Microsoft.Extensions.Logging;
using ZyRabbit.Operations.StateMachine.Middleware;
using ZyRabbit.Pipe.Middleware;
using System.Threading;
using System;

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
			var middleware = container.GetService<RetrieveStateMachineMiddleware>();

			/* Assert */
			Assert.NotNull(client);
			Assert.NotNull(middleware);
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
			Assert.NotNull(logger1);
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

		[Fact]
		public async Task Should_Be_Able_To_Resolve_Middleware_With_Parameter()
		{
			/* Setup */
			using var container = new SimpleDependencyInjection();
			container.AddZyRabbit();

			// Configure middleware via options to throw the InvalidOperationException exception
			var options = new ExchangeDeclareOptions
			{
				ThrowOnFailFunc = _ => true
			};

			/* Test */
			var middleware = container.GetService<ExchangeDeclareMiddleware>(options);

			/* Assert */
			await Assert.ThrowsAnyAsync<InvalidOperationException>(async () =>
			{
				await middleware.InvokeAsync(null, CancellationToken.None);
			});
		}
	}
}
