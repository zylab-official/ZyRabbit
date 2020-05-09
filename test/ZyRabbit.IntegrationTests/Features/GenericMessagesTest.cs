using System.Threading.Tasks;
using ZyRabbit.IntegrationTests.TestMessages;
using ZyRabbit.Pipe;
using Xunit;

namespace ZyRabbit.IntegrationTests.Features
{
	public class GenericMessagesTest
	{
		[Fact]
		public async void Should_Be_Able_To_Subscribe_To_Generic_Message()
		{
			using (var subscriber = ZyRabbitFactory.CreateTestClient())
			using (var publisher = ZyRabbitFactory.CreateTestClient())
			{
				/* Setup */
				var doneTsc = new TaskCompletionSource<GenericMessage<int>>();
				var message = new GenericMessage<int>
				{
					Prop = 7
				};
				await subscriber.SubscribeAsync<GenericMessage<int>>(received =>
					{
						doneTsc.TrySetResult(received);
						return Task.FromResult(0);
					}
				);
				/* Test */
				await publisher.PublishAsync(message);
				await doneTsc.Task;

				/* Assert */
				Assert.Equal(doneTsc.Task.Result.Prop, message.Prop);
			}
		}
	}
}
