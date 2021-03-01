using System;
using System.Linq;
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

		public IDependencyRegister AddSingleton(Type type, Type implementationType)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (implementationType == null)
				throw new ArgumentNullException(nameof(implementationType));

			_kernel.Bind(type).To(implementationType).InSingletonScope();
			return this;
		}

		public bool IsRegistered(Type type)
		{
			return _kernel.GetBindings(type).Any();
		}

		public IDependencyRegister Register(Type type, Type implementationType, Lifetime lifetime)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (implementationType == null)
				throw new ArgumentNullException(nameof(implementationType));

			var binding = _kernel.Bind(type).To(implementationType);
			ApplyLifetime(binding, lifetime);

			return this;
		}

		public IDependencyRegister Register<T>(Type type, Func<IDependencyResolver, T> instanceCreator, Lifetime lifetime) where T : class
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (instanceCreator == null)
				throw new ArgumentNullException(nameof(instanceCreator));

			var binding = _kernel.Bind(type).ToMethod(ctx => instanceCreator(new NinjectResolverAdapter(ctx)));
			ApplyLifetime(binding, lifetime);

			return this;
		}

		private static void ApplyLifetime<T>(global::Ninject.Syntax.IBindingWhenInNamedWithOrOnSyntax<T> binding, Lifetime lifetime)
		{
			switch (lifetime)
			{
				case Lifetime.Transient:
					{
						binding.InTransientScope();
						break;
					}
				case Lifetime.Singelton:
					{
						binding.InSingletonScope();
						break;
					}
				default:
					throw new NotSupportedException($"Lifetime '{lifetime}' is not supported");
			}
		}
	}
}
