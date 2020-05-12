using System;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using ZyRabbit.Enrichers.MessageContext;
using ZyRabbit.Enrichers.MessageContext.Context;
using ZyRabbit.Enrichers.MessageContext.Subscribe;
using ZyRabbit.Instantiation;
using ZyRabbit.IntegrationTests.TestMessages;
using ZyRabbit.Pipe;
using Xunit;

namespace ZyRabbit.IntegrationTests.Enrichers
{
	public class MessageContextTests
	{
		[Fact]
		public async Task Should_Send_Context_On_Rpc()
		{
			using (var requester = ZyRabbitFactory.CreateTestClient(new ZyRabbitOptions
			{
				Plugins = p => p.UseMessageContext(context => new MessageContext {GlobalRequestId = Guid.NewGuid()})
			}))
			using (var responder = ZyRabbitFactory.CreateTestClient())
			{
				/* Setup */
				MessageContext receivedContext = null;
				await responder.RespondAsync<BasicRequest, BasicResponse, MessageContext>((request, context) =>
					{
						receivedContext = context;
						return Task.FromResult(new BasicResponse());
					}
				);

				/* Test */
				await requester.RequestAsync<BasicRequest, BasicResponse>();

				/* Assert */
				Assert.NotNull(receivedContext);
			}
		}

		[Fact]
		public async Task Should_Send_Context_On_Pub_Sub()
		{
			using (
				var publisher =
					ZyRabbitFactory.CreateTestClient(new ZyRabbitOptions {Plugins = p => p.UseMessageContext<MessageContext>()}))
			using (var subscriber = ZyRabbitFactory.CreateTestClient())
			{
				/* Setup */
				var contextTsc = new TaskCompletionSource<MessageContext>();
				await subscriber.SubscribeAsync<BasicMessage, MessageContext>((request, context) =>
				{
					contextTsc.TrySetResult(context);
					return Task.FromResult(0);
				});

				/* Test */
				await publisher.PublishAsync(new BasicMessage());
				await contextTsc.Task;
				/* Assert */
				Assert.NotNull(contextTsc.Task);
			}
		}

		[Fact]
		public async Task Should_Override_With_Explicit_Context_On_Pub_Sub()
		{
			using (
				var publisher =
					ZyRabbitFactory.CreateTestClient(new ZyRabbitOptions {Plugins = p => p.UseMessageContext<MessageContext>()}))
			using (var subscriber = ZyRabbitFactory.CreateTestClient())
			{
				/* Setup */
				var contextTsc = new TaskCompletionSource<TestMessageContext>();
				await subscriber.SubscribeAsync<BasicMessage, TestMessageContext>((request, context) =>
				{
					contextTsc.TrySetResult(context);
					return Task.FromResult(0);
				});

				/* Test */
				await publisher.PublishAsync(new BasicMessage(), ctx => ctx.UseMessageContext(new TestMessageContext()));
				await contextTsc.Task;
				/* Assert */
				Assert.IsType<TestMessageContext>(contextTsc.Task.Result);
			}
		}

		[Fact]
		public async Task Shoud_Create_Context_From_Supplied_Factory_Method()
		{
			var contextProp = "Created from factory method";
			using (var publisher = ZyRabbitFactory.CreateTestClient(new ZyRabbitOptions
			{
				Plugins = p => p.UseMessageContext(context => new TestMessageContext {Prop = contextProp})
			}))
			using (var subscriber = ZyRabbitFactory.CreateTestClient())
			{
				/* Setup */
				var contextTsc = new TaskCompletionSource<TestMessageContext>();
				await subscriber.SubscribeAsync<BasicMessage, TestMessageContext>((request, context) =>
				{
					contextTsc.TrySetResult(context);
					return Task.FromResult(0);
				});

				/* Test */
				await publisher.PublishAsync(new BasicMessage());
				await contextTsc.Task;
				/* Assert */
				Assert.IsType<TestMessageContext>(contextTsc.Task.Result);
				Assert.Equal(((TestMessageContext) contextTsc.Task.Result).Prop, contextProp);
			}
		}

		[Fact]
		public async Task Should_Not_Forward_Context_By_Default()
		{
			var withMsgContext = new ZyRabbitOptions
			{
				Plugins = p => p.UseMessageContext(context =>
					new MessageContext
					{
						GlobalRequestId = Guid.NewGuid()
					})
			};
			using (var publisher = ZyRabbitFactory.CreateTestClient(withMsgContext))
			using (var subscriber = ZyRabbitFactory.CreateTestClient(withMsgContext))
			{
				/* Setup */
				var firstContextTsc = new TaskCompletionSource<MessageContext>();
				var secondContextTsc = new TaskCompletionSource<MessageContext>();
				await subscriber.SubscribeAsync<FirstMessage, MessageContext>((request, context) =>
				{
					firstContextTsc.TrySetResult(context);
					return subscriber.PublishAsync(new SecondMessage());
				});
				await subscriber.SubscribeAsync<SecondMessage, MessageContext>((message, context) =>
				{
					secondContextTsc.TrySetResult(context);
					return Task.FromResult(0);
				});

				/* Test */
				await publisher.PublishAsync(new FirstMessage());
				await firstContextTsc.Task;
				await secondContextTsc.Task;

				/* Assert */
				Assert.NotEqual(firstContextTsc.Task.Result.GlobalRequestId, secondContextTsc.Task.Result.GlobalRequestId);
			}
		}

		[Fact]
		public async Task Should_Forward_Context_On_Publish_With_Context_Forwarding()
		{
			var withMsgContext = new ZyRabbitOptions
			{
				Plugins = p => p
					.UseContextForwarding()
					.UseMessageContext(context =>
						new MessageContext
						{
							GlobalRequestId = Guid.NewGuid()
						})
			};
			using (var publisher = ZyRabbitFactory.CreateTestClient(withMsgContext))
			using (var subscriber = ZyRabbitFactory.CreateTestClient(withMsgContext))
			{
				/* Setup */
				var firstContextTsc = new TaskCompletionSource<MessageContext>();
				var secondContextTsc = new TaskCompletionSource<MessageContext>();
				await subscriber.SubscribeAsync<FirstMessage, MessageContext>((request, context) =>
				{
					firstContextTsc.TrySetResult(context);
					return subscriber.PublishAsync(new SecondMessage());
				});
				await subscriber.SubscribeAsync<SecondMessage, MessageContext>((message, context) =>
				{
					secondContextTsc.TrySetResult(context);
					return Task.FromResult(0);
				});

				/* Test */
				await publisher.PublishAsync(new FirstMessage());
				await firstContextTsc.Task;
				await secondContextTsc.Task;

				/* Assert */
				Assert.Equal(firstContextTsc.Task.Result.GlobalRequestId, secondContextTsc.Task.Result.GlobalRequestId);
			}
		}

		[Fact]
		public async Task Should_Forward_Context_For_Pub_Sub_And_Rpc()
		{
			var withMsgContext = new ZyRabbitOptions
			{
				Plugins = p => p
					.UseContextForwarding()
					.UseMessageContext(context =>
						new MessageContext
						{
							GlobalRequestId = Guid.NewGuid()
						})
			};
			using (var firstClient = ZyRabbitFactory.CreateTestClient(withMsgContext))
			using (var secondClient = ZyRabbitFactory.CreateTestClient(withMsgContext))
			{
				/* Setup */
				var firstContextTsc = new TaskCompletionSource<MessageContext>();
				var secondContextTsc = new TaskCompletionSource<MessageContext>();
				await secondClient.SubscribeAsync<FirstMessage, MessageContext>((request, context) =>
				{
					firstContextTsc.TrySetResult(context);
					return secondClient.RequestAsync<FirstRequest, FirstResponse>(new FirstRequest());
				});
				await secondClient.RespondAsync<FirstRequest, FirstResponse, MessageContext>((message, context) =>
				{
					secondContextTsc.TrySetResult(context);
					return Task.FromResult(new FirstResponse());
				});

				/* Test */
				await firstClient.PublishAsync(new FirstMessage());
				await firstContextTsc.Task;
				await secondContextTsc.Task;

				/* Assert */
				Assert.Equal(firstContextTsc.Task.Result.GlobalRequestId, secondContextTsc.Task.Result.GlobalRequestId);
			}
		}

		[Fact]
		public async Task Should_Be_Able_To_Have_Any_Object_As_Message_Context()
		{
			using (var publisher = ZyRabbitFactory.CreateTestClient())
			using (var subscriber = ZyRabbitFactory.CreateTestClient())
			{
				/* Setup */
				var contextTsc = new TaskCompletionSource<BasicDeliverEventArgs>();
				await subscriber.SubscribeAsync<BasicMessage, BasicDeliverEventArgs>((request, args) =>
				{
					contextTsc.TrySetResult(args);
					return Task.FromResult(0);
				}, ctx => ctx.UseMessageContext(c => c.GetDeliveryEventArgs()));

				/* Test */
				await publisher.PublishAsync(new BasicMessage());
				await contextTsc.Task;
				/* Assert */
				Assert.NotNull(contextTsc.Task);
			}
		}

		[Fact]
		public async Task Should_Use_Subscriber_Declared_Context()
		{
			using (var publisher = ZyRabbitFactory.CreateTestClient(new ZyRabbitOptions
			{
				Plugins = p => p.UseMessageContext<TestMessageContext>().UseContextForwarding()
			}))
			using (var subscriber = ZyRabbitFactory.CreateTestClient(new ZyRabbitOptions
			{
				Plugins = p => p.UseMessageContext<TestMessageContext>().UseContextForwarding()
			}))
			{
				/* Setup */
				var firstTcs = new TaskCompletionSource<TestMessageContext>();
				var secondTcs = new TaskCompletionSource<BasicDeliverEventArgs>();
				var thirdTcs = new TaskCompletionSource<TestMessageContext>();
				await subscriber.SubscribeAsync<FirstMessage, TestMessageContext>(async (request, ctx) =>
				{
					firstTcs.TrySetResult(ctx);
					await subscriber.PublishAsync(new SecondMessage());
				});
				await subscriber.SubscribeAsync<SecondMessage, BasicDeliverEventArgs>(async (request, args) =>
				{
					secondTcs.TrySetResult(args);
					await subscriber.PublishAsync(new ThirdMessage());
				}, ctx => ctx.UseMessageContext(c => c.GetDeliveryEventArgs()));
				await subscriber.SubscribeAsync<ThirdMessage, TestMessageContext>(async (request, ctx) =>
				{
					thirdTcs.TrySetResult(ctx);
				});

				/* Test */
				await publisher.PublishAsync(new FirstMessage());
				await firstTcs.Task;
				await secondTcs.Task;
				await thirdTcs.Task;

				/* Assert */
				Assert.NotNull(firstTcs.Task);
			}
		}
	}
}
