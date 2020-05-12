using System;
using System.Threading.Tasks;
using ZyRabbit.Configuration;
using ZyRabbit.Instantiation;
using Xunit;

namespace ZyRabbit.IntegrationTests.DependencyInjection
{
	public class SimpleDependencyTests
	{
		[Fact]
		public async Task Should_Honor_Client_Config_From_Options()
		{
			var config = ZyRabbitConfiguration.Local;
			const string nonExistingVhost = "/foo";
			config.VirtualHost = nonExistingVhost;
			await Assert.ThrowsAnyAsync<Exception>(async () =>
			{
				var factory = ZyRabbitFactory.CreateTestInstanceFactory(new ZyRabbitOptions {ClientConfiguration = config});
				var client = factory.Create();
				await client.CreateChannelAsync();
			});
		}
	}
}
