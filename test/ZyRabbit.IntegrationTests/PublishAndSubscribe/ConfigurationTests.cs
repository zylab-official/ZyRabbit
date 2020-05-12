using System;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using ZyRabbit.Common;
using ZyRabbit.Configuration.Exchange;
using ZyRabbit.Configuration.Queue;
using ZyRabbit.Enrichers.MessageContext.Subscribe;
using ZyRabbit.Enrichers.QueueSuffix;
using ZyRabbit.Instantiation;
using ZyRabbit.IntegrationTests.TestMessages;
using ZyRabbit.Pipe;
using ZyRabbit.Pipe.Middleware;
using Xunit;

namespace ZyRabbit.IntegrationTests.PublishAndSubscribe
{
	public class ConfigurationTests
	{
		[Fact]
		public async Task Should_Work_Without_Any_Additional_Configuration()
		{
			using (var publisher = ZyRabbitFactory.CreateTestClient())
			using (var subscriber = ZyRabbitFactory.CreateTestClient())
			{
				/* Setup */
				var receivedTcs = new TaskCompletionSource<BasicMessage>();
				await subscriber.SubscribeAsync<BasicMessage>(received =>
				{
					receivedTcs.TrySetResult(received);
					return Task.FromResult(true);
				});
				var message = new BasicMessage {Prop = "Hello, world!"};

				/* Test */
				await publisher.PublishAsync(message);
				await receivedTcs.Task;

				/* Assert */
				Assert.Equal(message.Prop, receivedTcs.Task.Result.Prop);
			}
		}

		[Fact]
		public async Task Should_Be_Able_To_Publish_With_Custom_Header()
		{
			using (var publisher = ZyRabbitFactory.CreateTestClient())
			using (var subscriber = ZyRabbitFactory.CreateTestClient())
			{
				/* Setup */
				var receivedTcs = new TaskCompletionSource<BasicDeliverEventArgs>();
				await subscriber.SubscribeAsync<BasicMessage, BasicDeliverEventArgs>((received, args) =>
				{
					receivedTcs.TrySetResult(args);
					return Task.FromResult(true);
				}, ctx => ctx.UseMessageContext(c => c.GetDeliveryEventArgs()));
				var message = new BasicMessage { Prop = "Hello, world!" };

				/* Test */
				await publisher.PublishAsync(message, ctx => ctx
					.UsePublishConfiguration(cfg => cfg
						.WithProperties(props => props.Headers.Add("foo", "bar"))));
				await receivedTcs.Task;

				/* Assert */
				Assert.True(receivedTcs.Task.Result.BasicProperties.Headers.ContainsKey("foo"));
			}
		}

		[Fact]
		public async Task Should_Honor_Exchange_Name_Configuration()
		{
			using (var publisher = ZyRabbitFactory.CreateTestClient())
			using (var subscriber = ZyRabbitFactory.CreateTestClient())
			{
				/* Setup */
				var receivedTcs = new TaskCompletionSource<BasicMessage>();
				await subscriber.SubscribeAsync<BasicMessage>(received =>
				{
					receivedTcs.TrySetResult(received);
					return Task.FromResult(true);
				}, ctx => ctx
					.UseSubscribeConfiguration(cfg => cfg
						.OnDeclaredExchange(e=> e
							.WithName("custom_exchange")
						))
				);

				var message = new BasicMessage { Prop = "Hello, world!" };

				/* Test */
				await publisher.PublishAsync(message, ctx => ctx.UsePublishConfiguration(cfg => cfg.OnExchange("custom_exchange")));
				await receivedTcs.Task;

				/* Assert */
				Assert.Equal(message.Prop, receivedTcs.Task.Result.Prop);
			}
		}

		[Fact]
		public async Task Should_Honor_Complex_Configuration()
		{
			using (var publisher = ZyRabbitFactory.CreateTestClient())
			using (var subscriber = ZyRabbitFactory.CreateTestClient())
			{
				/* Setup */
				var receivedTcs = new TaskCompletionSource<BasicMessage>();
				await subscriber.SubscribeAsync<BasicMessage>(received =>
				{
					receivedTcs.TrySetResult(received);
					return Task.FromResult(true);
				}, ctx => ctx
					.UseSubscribeConfiguration(cfg => cfg
						.Consume(c => c
							.WithRoutingKey("custom_key")
							.WithConsumerTag("custom_tag")
							.WithPrefetchCount(2)
							.WithNoLocal(false))
						.FromDeclaredQueue(q => q
							.WithName("custom_queue")
							.WithAutoDelete()
							.WithArgument(QueueArgument.DeadLetterExchange, "dlx"))
						.OnDeclaredExchange(e=> e
							.WithName("custom_exchange")
							.WithType(ExchangeType.Topic))
				));

				var message = new BasicMessage { Prop = "Hello, world!" };

				/* Test */
				await publisher.PublishAsync(message, ctx => ctx
					.UsePublishConfiguration(cfg => cfg
						.OnExchange("custom_exchange")
						.WithRoutingKey("custom_key")
				));
				await receivedTcs.Task;

				/* Assert */
				Assert.Equal(message.Prop, receivedTcs.Task.Result.Prop);
			}
		}

		[Fact]
		public async Task Should_Be_Able_To_Create_Unique_Queues_With_Naming_Suffix()
		{
			var options = new ZyRabbitOptions
			{
				Plugins = ioc => ioc
					.UseApplicationQueueSuffix()
					.UseQueueSuffix()
			};
			using (var firstSubscriber = ZyRabbitFactory.CreateTestClient(options))
			using (var secondSubscriber = ZyRabbitFactory.CreateTestClient(options))
			using (var publisher = ZyRabbitFactory.CreateTestClient())
			{
				/* Setup */
				var firstTcs = new TaskCompletionSource<BasicMessage>();
				var secondTcs = new TaskCompletionSource<BasicMessage>();
				var message = new BasicMessage {Prop = "I'm delivered twice."};
				await firstSubscriber.SubscribeAsync<BasicMessage>(msg =>
				{
					firstTcs.TrySetResult(msg);
					return Task.FromResult(0);
				}, ctx => ctx.UseCustomQueueSuffix("first")
				);

				await secondSubscriber.SubscribeAsync<BasicMessage>(msg =>
				{
					secondTcs.TrySetResult(msg);
					return Task.FromResult(0);
				}, ctx => ctx.UseCustomQueueSuffix("second")
				);

				/* Test */
				await publisher.PublishAsync(message);
				await firstTcs.Task;
				await secondTcs.Task;

				/* Assert */
				Assert.Equal(message.Prop, firstTcs.Task.Result.Prop);
				Assert.Equal(message.Prop, secondTcs.Task.Result.Prop);
			}
		}

		[Fact]
		public async Task Should_Not_Throw_Exception_When_Queue_Name_Is_Long()
		{
			using (var subscriber = ZyRabbitFactory.CreateTestClient())
			using (var publisher = ZyRabbitFactory.CreateTestClient())
			{
				/* Setup */
				var msgTcs = new TaskCompletionSource<BasicMessage>();
				var message = new BasicMessage { Prop = "I'm delivered to queue with truncated name" };
				var queueName = string.Empty;
				while (queueName.Length < 254)
				{
					queueName = queueName + "this_is_part_of_a_long_queue_name";
				}
				await subscriber.SubscribeAsync<BasicMessage>(msg =>
				{
					msgTcs.TrySetResult(msg);
					return Task.FromResult(0);
				}, ctx => ctx
					.UseSubscribeConfiguration(cfg => cfg
						.FromDeclaredQueue(q => q.WithName(queueName).WithAutoDelete())
					)
				);

				/* Test */
				await publisher.PublishAsync(message);
				await msgTcs.Task;

				/* Assert */
				Assert.Equal(message.Prop, msgTcs.Task.Result.Prop);
			}
		}

		[Fact]
		public async Task Should_Not_Throw_Exception_When_Exchange_Name_Is_Long()
		{
			using (var subscriber = ZyRabbitFactory.CreateTestClient())
			using (var publisher = ZyRabbitFactory.CreateTestClient())
			{
				/* Setup */
				var msgTcs = new TaskCompletionSource<BasicMessage>();
				var message = new BasicMessage { Prop = "I'm delivered on an exchange with truncated name" };
				var exchangeName = string.Empty;
				while (exchangeName.Length < 254)
				{
					exchangeName = exchangeName + "this_is_part_of_a_long_exchange_name";
				}

				await subscriber.SubscribeAsync<BasicMessage>(msg =>
				{
					msgTcs.TrySetResult(msg);
					return Task.FromResult(0);
				}, ctx => ctx
					.UseSubscribeConfiguration(cfg => cfg
						.OnDeclaredExchange(e => e.WithName(exchangeName).WithAutoDelete())
					)
				);

				/* Test */
				await publisher.PublishAsync(message, ctx => ctx
					.UsePublishAcknowledge()
					.UsePublishConfiguration(c => c.OnExchange(exchangeName))
				);
				await msgTcs.Task;

				/* Assert */
				Assert.Equal(message.Prop, msgTcs.Task.Result.Prop);
			}
		}

		[Fact]
		public async Task Should_Consume_Message_Already_In_Queue()
		{
			using (var subscriber = ZyRabbitFactory.CreateTestClient())
			using (var publisher = ZyRabbitFactory.CreateTestClient())
			{
				var msgTcs = new TaskCompletionSource<BasicMessage>();
				var msg = new BasicMessage { Prop = Guid.NewGuid().ToString() };
				await subscriber.DeclareQueueAsync<BasicMessage>();
				await subscriber.DeclareExchangeAsync<BasicMessage>();
				await subscriber.BindQueueAsync<BasicMessage>();
				await publisher.PublishAsync(msg);
				await subscriber.SubscribeAsync<BasicMessage>(message =>
				{
					msgTcs.TrySetResult(message);
					return Task.FromResult(true);
				});
				await msgTcs.Task;
				Assert.Equal(msg.Prop, msgTcs.Task.Result.Prop);
			}
		}

		[Fact]
		public async Task Should_Be_Able_To_Declare_Exchange_And_Queue_Using_Declaration_Object()
		{
			const string ExchangeName = "zyrabbit.integrationtests.testmessages.declaration.object";
			const string QueueName = "declaration.object.queue";
			const string RoutingKey = "#";

			using (var subscriber = ZyRabbitFactory.CreateTestClient())
			using (var publisher = ZyRabbitFactory.CreateTestClient())
			{
				var msgTcs = new TaskCompletionSource<BasicMessage>();
				var msg = new BasicMessage { Prop = Guid.NewGuid().ToString() };
				await subscriber.DeclareExchangeAsync(new ExchangeDeclaration()
				{
					Name = ExchangeName,
					ExchangeType = "fanout",
					AutoDelete = true
				});
				await subscriber.DeclareQueueAsync(new QueueDeclaration()
				{
					Name = QueueName,
					AutoDelete = true
				});
				await subscriber.BindQueueAsync(QueueName, ExchangeName, RoutingKey);

				await publisher.PublishAsync(msg, ctx => ctx
					.UsePublishConfiguration(cfg => cfg
					.OnExchange(ExchangeName)));

				await subscriber.SubscribeAsync<BasicMessage>(message =>
				{
					msgTcs.TrySetResult(message);
					return Task.FromResult(true);
				}, ctx => ctx.UseSubscribeConfiguration(cfg => cfg
					.Consume(consume => consume
						.OnExchange(ExchangeName)
						.FromQueue(QueueName)
						.WithRoutingKey(RoutingKey))));

				await msgTcs.Task;
				Assert.Equal(msg.Prop, msgTcs.Task.Result.Prop);
			}
		}
	}
}
