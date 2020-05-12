using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZyRabbit.Instantiation;
using ZyRabbit.IntegrationTests.StateMachine.Generic;
using ZyRabbit.Operations.StateMachine;
using Xunit;

namespace ZyRabbit.IntegrationTests.StateMachine
{
	public class StateMachineTests
	{
		[Fact]
		public async Task Should_Complete_Generic_Task()
		{
			using (var processOwner = ZyRabbitFactory.CreateTestClient(new ZyRabbitOptions { Plugins = p => p.UseStateMachine() }))
			using (var worker = ZyRabbitFactory.CreateTestClient())
			using (var initiator = ZyRabbitFactory.CreateTestClient())
			using (var observer = ZyRabbitFactory.CreateTestClient())
			{
				var tsc = new TaskCompletionSource<ProcessCompeted>();
				var updates = new List<ProcessUpdated>();
				await processOwner.RegisterStateMachineAsync<ProcessTriggers>();
				await observer.SubscribeAsync<ProcessCompeted>(competed =>
				{
					tsc.TrySetResult(competed);
					return Task.FromResult(0);
				});
				await observer.SubscribeAsync<ProcessUpdated>(updated =>
				{
					updates.Add(updated);
					return Task.FromResult(0);
				});
				await worker.SubscribeAsync<TaskCreated>(async msg =>
				{
					await worker.PublishAsync(new StartTask
					{
						Assignee = "Luke Skyworker",
						TaskId = msg.TaskId
					});
					await Task.Delay(TimeSpan.FromMilliseconds(30));
					await worker.PublishAsync(new PauseTask
					{
						TaskId = msg.TaskId,
						Reason = "Need to repair 3CPO first."
					});
					await Task.Delay(TimeSpan.FromMilliseconds(30));
					await worker.PublishAsync(new ResumeTask
					{
						TaskId = msg.TaskId,
						Message = "Back to work"
					});
					await Task.Delay(TimeSpan.FromMilliseconds(30));
					await worker.PublishAsync(new CompleteTask
					{
						TaskId = msg.TaskId
					});
				});
				await initiator.PublishAsync(new CreateTask
				{
					Name = "Destroy Death Star.",
					DeadLine = DateTime.Today.AddDays(2)
				});
				await tsc.Task;
				Assert.True(true);
			}
		}
	}
}
