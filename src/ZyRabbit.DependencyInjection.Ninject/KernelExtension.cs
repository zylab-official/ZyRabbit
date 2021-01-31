using Ninject;
using System;
using ZyRabbit.Instantiation;

namespace ZyRabbit.DependencyInjection.Ninject
{
	public static class KernelExtension
	{
		public static IKernel RegisterZyRabbit(this IKernel kernel, ZyRabbitOptions options = null)
		{
			if (kernel == null)
				throw new ArgumentNullException(nameof(kernel));

			// It is required for injecting options to differnt middlewares
			kernel.Settings.AllowNullInjection = true;

			var adapter = new NinjectRegisterAdapter(kernel);
			adapter.AddZyRabbit(options);
			return kernel;
		}
	}
}
