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
		private Action<MessageA> _subscribeWithoutContext;
		private MessageA _messageA = new MessageA();
		private IBusClient _withContext;
		private Action<MessageB> _subscribeWithContext;
		private MessageB _messageB = new MessageB();

		[GlobalSetup]
		public void Setup()
		{
			_withoutContext = ZyRabbitFactory.CreateSingleton();
			_withContext = ZyRabbitFactory.CreateSingleton(new ZyRabbitOptions
			{
				Plugins = p => p.UseMessageContext<MessageContext>()
			});
			_withoutContext.SubscribeAsync<MessageA>(message =>
			{
				_subscribeWithoutContext(message);
				return Task.CompletedTask;
			});
			_withContext.SubscribeAsync<MessageB, MessageContext>((message, context) =>
			{
				_subscribeWithContext(message);
				return Task.CompletedTask;
			});
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			_withoutContext.DeleteQueueAsync<MessageA>();
			_withContext.DeleteQueueAsync<MessageB>();
			(_withoutContext as IDisposable).Dispose();
			(_withContext as IDisposable).Dispose();
		}

		[Benchmark]
		public async Task MessageContext_FromFactory()
		{
			var msgTsc = new TaskCompletionSource<MessageB>();
			_subscribeWithContext = message => msgTsc.SetResult(message);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			_withContext.PublishAsync(_messageB);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			await msgTsc.Task;
		}

		[Benchmark]
		public async Task MessageContext_None()
		{
			var msgTsc = new TaskCompletionSource<MessageA>();
			_subscribeWithoutContext = message => msgTsc.SetResult(message);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			_withoutContext.PublishAsync(_messageA);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			await msgTsc.Task;
		}

		public class MessageA { }
		public class MessageB { }
		public class MessageContext { }
	}
}
