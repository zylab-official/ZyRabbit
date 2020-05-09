using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using ZyRabbit.IntegrationTests.TestMessages;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;
using Xunit;

namespace ZyRabbit.IntegrationTests.Features
{
	public class GracefulShutdownTest : IntegrationTestBase
	{
		[Fact]
		public async Task Should_Cancel_Subscription_When_Shutdown_Is_Called()
		{
			var singleton = ZyRabbitFactory.CreateTestClient();
			var instanceFactory = ZyRabbitFactory.CreateTestInstanceFactory();
			var client = instanceFactory.Create();
			var processMs =50;
			var firstMsg = (new BasicMessage { Prop = "I'll get processed" });
			var secondMsg = (new BasicMessage { Prop = "I'll get stuck in the queue" });

			var firstTsc = new TaskCompletionSource<BasicMessage>();
			await client.SubscribeAsync<BasicMessage>(async message =>
			{
				firstTsc.TrySetResult(message);
				await Task.Delay(processMs);
			}, ctx => ctx
				.UseSubscribeConfiguration(cfg => cfg
					.FromDeclaredQueue(q => q
						.WithAutoDelete(false)
				)
			));

			await client.PublishAsync(firstMsg);
			await firstTsc.Task;
			var shutdownTask = instanceFactory.ShutdownAsync(TimeSpan.FromMilliseconds(processMs));
			await Task.Delay(TimeSpan.FromMilliseconds(10));
			await singleton.PublishAsync(secondMsg);
			await shutdownTask;

			var secondReceived = await singleton.GetAsync<BasicMessage>(get => get.WithAutoAck());
			await singleton.DeleteQueueAsync<BasicMessage>();
			await singleton.DeleteExchangeAsync<BasicMessage>();
			singleton.Dispose();

			Assert.Equal(firstMsg.Prop, firstTsc.Task.Result.Prop);
			Assert.Equal(secondMsg.Prop, secondReceived.Content.Prop);
		}
	}
}
