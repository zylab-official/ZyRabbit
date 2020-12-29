using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using ZyRabbit.Instantiation;

namespace ZyRabbit.PerformanceTest
{
	public class PubSubBenchmarks
	{
		private IBusClient _busClient;
		private Message _message = new Message();
		private Action<Message> _subscribe;

		[GlobalSetup]
		public void Setup()
		{
			_busClient = ZyRabbitFactory.CreateSingleton();
			_busClient.SubscribeAsync<Message>(message =>
			{
				_subscribe(message);
				return Task.CompletedTask;
			});
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			_busClient.DeleteQueueAsync<Message>();
			(_busClient as IDisposable).Dispose();
		}

		[Benchmark]
		public async Task ConsumerAcknowledgements_Off()
		{
			var msgTsc = new TaskCompletionSource<Message>();
			_subscribe = message => msgTsc.SetResult(message);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			_busClient.PublishAsync(_message, ctx => ctx.UsePublishAcknowledge(false));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			await msgTsc.Task;
		}

		[Benchmark]
		public async Task ConsumerAcknowledgements_On()
		{
			var msgTsc = new TaskCompletionSource<Message>();
			_subscribe = message => msgTsc.SetResult(message);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			_busClient.PublishAsync(_message);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			await msgTsc.Task;
		}

		[Benchmark]
		public async Task DeliveryMode_NonPersistant()
		{
			var msgTsc = new TaskCompletionSource<Message>();
			_subscribe = message => msgTsc.SetResult(message);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			_busClient.PublishAsync(_message, ctx => ctx
				.UsePublishConfiguration(cfg => cfg
					.WithProperties(p => p.DeliveryMode = 1))
			);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			await msgTsc.Task;
		}

		[Benchmark]
		public async Task DeliveryMode_Persistant()
		{
			var msgTsc = new TaskCompletionSource<Message>();
			_subscribe = message => msgTsc.SetResult(message);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			_busClient.PublishAsync(_message, ctx => ctx
				.UsePublishConfiguration(cfg => cfg
					.WithProperties(p => p.DeliveryMode = 2))
			);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			await msgTsc.Task;
		}

		public class Message { }
	}
}
