using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Instantiation;
using ZyRabbit.Operations.StateMachine;
using ZyRabbit.Operations.StateMachine.Core;

namespace ZyRabbit
{
	public static class StateMachinePlugin
	{
		/// <summary>
		/// Registers dependencies for the StateMachine operation.
		///
		/// StateMachine models are stored in-memory, but can
		/// be changed by registrating functions for get, add and update.
		/// The execution is only exclusive in the process. If usesd in
		/// a distributed environment, consider changing this by
		/// register an own execute func.
		/// </summary>
		/// <param name="builder">The client builder</param>
		/// <param name="get">Get for model (repo)</param>
		/// <param name="addOrUpdate">Add or update (repo)</param>
		/// <param name="execute">Method to ensure exclusive execution</param>
		/// <returns></returns>
		public static IClientBuilder UseStateMachine(
			this IClientBuilder builder,
			Func<Guid, Task<Model>> get = null,
			Func<Model, Task> addOrUpdate = null,
			Func<Guid, Func<Task>, CancellationToken, Task> execute = null)
		{
			builder.Register(
				pipe => {},
				ioc => ioc
					.AddSingleton<IGlobalLock, GlobalLock>(resolver => new GlobalLock(resolver.GetService<ILogger<IGlobalLock>>(), execute))
					.AddSingleton<IModelRepository>(new ModelRepository(get, addOrUpdate))
					.AddTransient<IStateMachineActivator, StateMachineActivator>(resolver => new StateMachineActivator(resolver.GetService<IModelRepository>(), resolver))
				);
			return builder;
		}
	}
}
