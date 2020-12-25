using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using ZyRabbit.Enrichers.MessageContext;
using ZyRabbit.Instantiation;

namespace ZyRabbit.PerformanceTest
{
	public class MessageContextBenchmarks
	{
		private IBusClient _withoutContext;
		private MessageA _messageA;
		private IBusClient _withContext;
		private MessageB _messageB;

		[GlobalSetup]
		public void Setup()
		{
			_withoutContext = ZyRabbitFactory.CreateSingleton();
			_withContext = ZyRabbitFactory.CreateSingleton(new ZyRabbitOptions
			{
				Plugins = p => p.UseMessageContext<MessageContext>()
			});
			_messageA = new MessageA();
			_messageB = new MessageB();
			_withoutContext.SubscribeAsync<MessageA>(message =>
			{
				return Task.CompletedTask;
			});
			_withContext.SubscribeAsync<MessageB, MessageContext>((message, context) =>
			{
				return Task.CompletedTask;
			});
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			_withoutContext.DeleteQueueAsync<MessageA>();
			_withoutContext.DeleteQueueAsync<MessageB>();
			(_withoutContext as IDisposable).Dispose();
			(_withContext as IDisposable).Dispose();
		}

		[Benchmark]
		public async Task MessageContext_FromFactory()
		{
			await _withContext.PublishAsync(_messageB);
		}

		[Benchmark]
		public async Task MessageContext_None()
		{
			await _withoutContext.PublishAsync(_messageA);
		}

		public class MessageA { }
		public class MessageB { }
		public class MessageContext { }
	}
}
