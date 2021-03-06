using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using ZyRabbit.Common;
using ZyRabbit.Configuration;
using ZyRabbit.DependencyInjection;
using ZyRabbit.DependencyInjection.Autofac;
using ZyRabbit.Instantiation;
using ZyRabbit.IntegrationTests.TestMessages;
using Xunit;
using Microsoft.Extensions.Logging;
using ZyRabbit.Operations.StateMachine.Middleware;
using ZyRabbit.Pipe.Middleware;
using System;
using System.Threading;

namespace ZyRabbit.IntegrationTests.DependencyInjection
{
	public sealed class AutofacTests
	{
		[Fact]
		public async Task Should_Be_Able_To_Publish_Message_From_Resolved_Client()
		{
			/* Setup */
			var builder = new ContainerBuilder();
			builder.RegisterZyRabbit();
			using var container = builder.Build();
			var adapter = new ComponentContextAdapter(container);

			/* Test */
			var client = adapter.GetService<IBusClient>();
			await client.PublishAsync(new BasicMessage());
			await client.DeleteExchangeAsync<BasicMessage>();
			var disposer = adapter.GetService<IResourceDisposer>();

			/* Assert */
			disposer.Dispose();
		}

		[Fact]
		public async Task Should_Honor_Client_Configuration()
		{
			/* Setup */
			var builder = new ContainerBuilder();
			var config = ZyRabbitConfiguration.Local;
			config.VirtualHost = "/foo";

			/* Test */
			await Assert.ThrowsAsync<DependencyResolutionException>(async () =>
			{
				builder.RegisterZyRabbit(new ZyRabbitOptions
				{
					ClientConfiguration = config
				});
				using var container = builder.Build();
				var adapter = new ComponentContextAdapter(container);

				var client = adapter.GetService<IBusClient>();
				await client.CreateChannelAsync();
			});
		}

		[Fact]
		public void Should_Be_Able_To_Resolve_Client_With_Plugins_From_Autofac()
		{
			/* Setup */
			var builder = new ContainerBuilder();
			builder.RegisterZyRabbit(new ZyRabbitOptions
			{
				Plugins = p => p.UseStateMachine()
			});
			using var container = builder.Build();
			var adapter = new ComponentContextAdapter(container);

			/* Test */
			var client = adapter.GetService<IBusClient>();
			var middleware = adapter.GetService<RetrieveStateMachineMiddleware>();

			/* Assert */
			Assert.NotNull(client);
			Assert.NotNull(middleware);
		}

		[Fact]
		public void Should_Be_Able_To_Resolve_Logger()
		{
			/* Setup */
			var builder = new ContainerBuilder();
			builder.RegisterZyRabbit();
			using var container = builder.Build();
			var adapter = new ComponentContextAdapter(container);

			/* Test */
			var logger1 = adapter.GetService<ILogger<IExclusiveLock>>();
			var logger2 = adapter.GetService<ILogger<IExclusiveLock>>();

			/* Assert */
			Assert.Same(logger1, logger2);
			Assert.NotNull(logger1);
		}

		[Fact]
		public async Task Should_Be_Able_To_Resolve_Middleware_With_Parameter()
		{
			/* Setup */
			var builder = new ContainerBuilder();
			builder.RegisterZyRabbit();
			using var container = builder.Build();
			var adapter = new ComponentContextAdapter(container);

			// Configure middleware via options to throw the InvalidOperationException exception
			var options = new ExchangeDeclareOptions
			{
				ThrowOnFailFunc = _ => true
			};

			/* Test */
			var middleware = adapter.GetService<ExchangeDeclareMiddleware>(options);

			/* Assert */
			await Assert.ThrowsAnyAsync<InvalidOperationException>(async () =>
			{
				await middleware.InvokeAsync(null, CancellationToken.None);
			});
		}
	}
}
