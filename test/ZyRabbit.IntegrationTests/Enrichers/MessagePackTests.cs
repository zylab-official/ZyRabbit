using System.Threading.Tasks;
using MessagePack;
using ZyRabbit.Enrichers.MessagePack;
using ZyRabbit.Instantiation;
using Xunit;

namespace ZyRabbit.IntegrationTests.Enrichers
{
	public class MessagePackTests
	{
		[Fact]
		public async Task Should_Publish_And_Subscribe_with_Zero_Formatter()
		{
			using (var client = ZyRabbitFactory.CreateTestClient(new ZyRabbitOptions { Plugins = p => p.UseMessagePack() }))
			{
				/** Setup **/
				var tcs = new TaskCompletionSource<MessagePackMessage>();
				var message = new MessagePackMessage
				{
					TagLine = "Extremely Fast MessagePack Serializer for C#"
				};
				await client.SubscribeAsync<MessagePackMessage>(msg =>
				{
					tcs.TrySetResult(msg);
					return Task.CompletedTask;
				});

				/** Test **/
				await client.PublishAsync(message);
				await tcs.Task;

				/** Assert **/
				Assert.Equal(tcs.Task.Result.TagLine, message.TagLine);
			}
		}
	}

	[MessagePackObject]
	public class MessagePackMessage
	{
		[Key(0)]
		public string TagLine { get; set; }
	}
}
