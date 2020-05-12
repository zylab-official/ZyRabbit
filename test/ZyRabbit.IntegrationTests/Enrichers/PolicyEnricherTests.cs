using System;
using System.Threading.Tasks;
using Polly;
using RabbitMQ.Client.Exceptions;
using ZyRabbit.Configuration.Queue;
using ZyRabbit.Enrichers.Polly;
using ZyRabbit.Instantiation;
using ZyRabbit.IntegrationTests.TestMessages;
using ZyRabbit.Pipe;
using Xunit;

namespace ZyRabbit.IntegrationTests.Enrichers
{
	public class PolicyEnricherTests
	{
		[Fact]
		public async Task Should_Use_Custom_Policy()
		{
			var defaultCalled = false;
			var customCalled = false;
			var defaultPolicy = Policy
				.Handle<Exception>()
				.FallbackAsync(ct =>
				{
					defaultCalled = true;
					return Task.FromResult(0);
				});
			var declareQueuePolicy = Policy
				.Handle<OperationInterruptedException>()
				.RetryAsync(async (e, retryCount, ctx) =>
				{
					customCalled = true;
					var defaultQueueCfg = ctx.GetPipeContext().GetClientConfiguration().Queue;
					var topology = ctx.GetTopologyProvider();
					var queue = new QueueDeclaration(defaultQueueCfg) { Name = ctx.GetQueueName(), Durable = false};
					await topology.DeclareQueueAsync(queue);
				});

			var options = new ZyRabbitOptions
			{
				Plugins = p => p.UsePolly(c => c
					.UsePolicy(defaultPolicy)
					.UsePolicy(declareQueuePolicy, PolicyKeys.QueueBind)
				)
			};

			using (var client = ZyRabbitFactory.CreateTestClient(options))
			{
				await client.SubscribeAsync<BasicMessage>(
					message => Task.FromResult(0),
					ctx => ctx.UseSubscribeConfiguration(cfg => cfg
						.Consume(c => c
							.FromQueue("does_not_exist"))
					));
			}

			Assert.True(customCalled);
			Assert.False(defaultCalled, "The custom retry policy should be called");
		}
	}
}
