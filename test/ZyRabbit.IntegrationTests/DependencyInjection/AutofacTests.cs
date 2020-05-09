using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using ZyRabbit.Common;
using ZyRabbit.Configuration;
using ZyRabbit.DependencyInjection.Autofac;
using ZyRabbit.Instantiation;
using ZyRabbit.IntegrationTests.TestMessages;
using ZyRabbit.Logging;
using Xunit;

namespace ZyRabbit.IntegrationTests.DependencyInjection
{
	public class AutofacTests
	{
		[Fact]
		public async Task Should_Be_Able_To_Resolve_Client_From_Autofac()
		{
			/* Setup */
			var builder = new ContainerBuilder();
			builder.RegisterZyRabbit();
			var container = builder.Build();

			/* Test */
			var client = container.Resolve<IBusClient>();
			var disposer = container.Resolve<IResourceDisposer>();

			/* Assert */
			disposer.Dispose();
			Assert.True(true);
		}

		[Fact]
		public async Task Should_Be_Able_To_Publish_Message_From_Resolved_Client()
		{
			/* Setup */
			var builder = new ContainerBuilder();
			builder.RegisterZyRabbit();
			var container = builder.Build();

			/* Test */
			var client = container.Resolve<IBusClient>();
			await client.PublishAsync(new BasicMessage());
			await client.DeleteExchangeAsync<BasicMessage>();
			var disposer = container.Resolve<IResourceDisposer>();

			/* Assert */
			disposer.Dispose();
			Assert.True(true);
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
				var container = builder.Build();
				var client = container.Resolve<IBusClient>();
				await client.CreateChannelAsync();
			});


			/* Assert */
			Assert.True(true);
		}

		[Fact]
		public async Task Should_Be_Able_To_Resolve_Client_With_Plugins_From_Autofac()
		{
			/* Setup */
			var builder = new ContainerBuilder();
			builder.RegisterZyRabbit(new ZyRabbitOptions
			{
				Plugins = p => p.UseStateMachine()
			});
			var container = builder.Build();

			/* Test */
			var client = container.Resolve<IBusClient>();
			var disposer = container.Resolve<IResourceDisposer>();

			/* Assert */
			disposer.Dispose();
			Assert.True(true);
		}
	}
}
