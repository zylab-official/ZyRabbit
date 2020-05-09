using System;
using ZyRabbit.Operations.MessageSequence.Configuration.Abstraction;
using ZyRabbit.Operations.MessageSequence.Model;
using ZyRabbit.Operations.StateMachine;
using ZyRabbit.Operations.StateMachine.Middleware;

namespace ZyRabbit.Operations.MessageSequence
{
	public static class MessageSequenceExtension
	{
		public static MessageSequence<TCompleteType> ExecuteSequence<TCompleteType>(
			this IBusClient client,
			Func<IMessageChainPublisher, MessageSequence<TCompleteType>> cfg
		)
		{
			var sequenceMachine = client
				.InvokeAsync(ctx => ctx
					.Use<RetrieveStateMachineMiddleware>(new RetrieveStateMachineOptions
					{
						StateMachineTypeFunc = pipeContext => typeof(StateMachine.MessageSequence)
					})
				)
				.GetAwaiter()
				.GetResult()
				.GetStateMachine();

			return cfg((StateMachine.MessageSequence)sequenceMachine);
		}
	}
}
