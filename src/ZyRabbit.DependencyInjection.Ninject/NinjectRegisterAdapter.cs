using System;
using Ninject;

namespace ZyRabbit.DependencyInjection.Ninject
{
	public class NinjectRegisterAdapter : IDependencyRegister
	{
		private readonly IKernel _kernel;

		public NinjectRegisterAdapter(IKernel kernel)
		{
			_kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
		}

		public IDependencyRegister AddSingleton<TService>(TService instance) where TService : class
		{
			_kernel.Bind<TService>().ToConstant(instance);
			return this;
		}

		public IDependencyRegister AddSingleton(Type type, Type implementationType)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (implementationType == null)
				throw new ArgumentNullException(nameof(implementationType));

			_kernel.Bind(type).To(implementationType).InSingletonScope();
			return this;
		}

		IDependencyRegister IDependencyRegister.AddSingleton<TService, TImplementation>(Func<IDependencyResolver, TService> instanceCreator)
		{
			if (instanceCreator == null)
				throw new ArgumentNullException(nameof(instanceCreator));

			_kernel.Bind<TService>().ToMethod(ctx => instanceCreator(new NinjectResolverAdapter(ctx))).InSingletonScope();
			return this;
		}

		IDependencyRegister IDependencyRegister.AddSingleton<TService, TImplementation>()
		{
			_kernel.Bind<TService>().To<TImplementation>().InSingletonScope();
			return this;
		}

		IDependencyRegister IDependencyRegister.AddTransient<TService, TImplementation>(Func<IDependencyResolver, TImplementation> instanceCreator)
		{
			if (instanceCreator == null)
				throw new ArgumentNullException(nameof(instanceCreator));

			_kernel.Bind<TService>().ToMethod(ctx => instanceCreator(new NinjectResolverAdapter(ctx))).InTransientScope();
			return this;
		}

		IDependencyRegister IDependencyRegister.AddTransient<TService, TImplementation>()
		{
			_kernel.Bind<TService>().To<TImplementation>().InTransientScope();
			return this;
		}
	}
}
