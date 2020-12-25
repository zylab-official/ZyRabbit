using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using ZyRabbit.Instantiation;

namespace ZyRabbit.PerformanceTest
{
	public class PubSubBenchmarks
	{
		private IBusClient _busClient;
		private Message _message;

		[GlobalSetup]
		public void Setup()
		{
			_busClient = ZyRabbitFactory.CreateSingleton();
			_message = new Message();
			_busClient.SubscribeAsync<Message>(message =>
			{
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
			await _busClient.PublishAsync(_message, ctx => ctx.UsePublishAcknowledge(false));
		}

		[Benchmark]
		public async Task ConsumerAcknowledgements_On()
		{
			await _busClient.PublishAsync(_message);
		}

		[Benchmark]
		public async Task DeliveryMode_NonPersistant()
		{
			await _busClient.PublishAsync(_message, ctx => ctx
				.UsePublishConfiguration(cfg => cfg
					.WithProperties(p => p.DeliveryMode = 1))
			);
		}

		[Benchmark]
		public async Task DeliveryMode_Persistant()
		{
			await _busClient.PublishAsync(_message, ctx => ctx
				.UsePublishConfiguration(cfg => cfg
					.WithProperties(p => p.DeliveryMode = 2))
			);
		}
	}

	public class Message { }
}
