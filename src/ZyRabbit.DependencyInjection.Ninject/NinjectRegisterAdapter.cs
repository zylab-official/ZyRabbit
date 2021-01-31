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

		public IDependencyRegister AddSingleton(Type type, Func<IDependencyResolver, Type, object> instanceCreator)
		{
			_kernel.Bind(type).ToMethod(ctx =>
			{
				return instanceCreator(new NinjectResolverAdapter(ctx), ctx.Request.Service);
			}).InSingletonScope();
			return this;
		}

		public IDependencyRegister AddSingleton<TService>(TService instance) where TService : class
		{
			_kernel.Bind<TService>().ToConstant(instance);
			return this;
		}

		public IDependencyRegister AddTransient(Type type, Func<IDependencyResolver, Type, object> instanceCreator)
		{
			_kernel.Bind(type).ToMethod(ctx => instanceCreator(new NinjectResolverAdapter(ctx), type)).InTransientScope();
			return this;
		}

		IDependencyRegister IDependencyRegister.AddSingleton<TService, TImplementation>(Func<IDependencyResolver, TService> instanceCreator)
		{
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
