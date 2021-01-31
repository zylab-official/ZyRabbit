using System.Threading.Tasks;
using Ninject;
using ZyRabbit.DependencyInjection.Ninject;
using ZyRabbit.Instantiation;
using Xunit;
using ZyRabbit.IntegrationTests.TestMessages;
using ZyRabbit.Common;
using ZyRabbit.Configuration;
using System;
using RabbitMQ.Client.Exceptions;

namespace ZyRabbit.IntegrationTests.DependencyInjection
{
	public class NinjectTests
	{
		[Fact]
		public async Task Should_Be_Able_To_Publish_Message_From_Resolved_Client()
		{
			/* Setup */
			var kernel = new StandardKernel();
			kernel.RegisterZyRabbit();

			/* Test */
			var client = kernel.Get<IBusClient>();
			await client.PublishAsync(new BasicMessage());
			await client.DeleteExchangeAsync<BasicMessage>();
			var disposer = kernel.Get<IResourceDisposer>();

			/* Assert */
			disposer.Dispose();
		}

		[Fact]
		public async Task Should_Honor_Client_Configuration()
		{
			/* Setup */
			var kernel = new StandardKernel();
			var config = ZyRabbitConfiguration.Local;
			config.VirtualHost = "/foo";

			/* Test */
			await Assert.ThrowsAsync<BrokerUnreachableException>(async () =>
			{
				kernel.RegisterZyRabbit(new ZyRabbitOptions
				{
					ClientConfiguration = config
				});
				var client = kernel.Get<IBusClient>();
				await client.CreateChannelAsync();
			});
		}

		[Fact]
		public void Should_Be_Able_To_Resolve_Client_With_Plugins_From_Ninject()
		{
			/* Setup */
			var kernel = new StandardKernel();
			kernel.RegisterZyRabbit(new ZyRabbitOptions
			{
				Plugins = p => p.UseStateMachine()
			});

			/* Test */
			var client = kernel.Get<IBusClient>();
			var disposer = kernel.Get<IResourceDisposer>();

			/* Assert */
			disposer.Dispose();
		}
	}
}
