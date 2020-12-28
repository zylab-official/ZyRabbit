using System;
using Autofac;

namespace ZyRabbit.DependencyInjection.Autofac
{
	public class ContainerBuilderAdapter : IDependencyRegister
	{
		private readonly ContainerBuilder _builder;

		public ContainerBuilderAdapter(ContainerBuilder builder)
		{
			_builder = builder;
		}

		public IDependencyRegister AddTransient<TService, TImplementation>(Func<IDependencyResolver, TImplementation> instanceCreator) where TService : class where TImplementation : class, TService
		{
			_builder
				.Register<TImplementation>(context => instanceCreator(new ComponentContextAdapter(context.Resolve<IComponentContext>())))
				.As<TService>()
				.InstancePerDependency();
			return this;
		}

		public IDependencyRegister AddTransient<TService, TImplementation>() where TService : class where TImplementation : class, TService
		{
			_builder
				.RegisterType<TImplementation>()
				.As<TService>()
				.InstancePerDependency();
			return this;
		}

		public IDependencyRegister AddSingleton<TService>(TService instance) where TService : class
		{
			_builder
				.Register<TService>(context => instance)
				.As<TService>()
				.SingleInstance();
			return this;
		}

		public IDependencyRegister AddSingleton<TService, TImplementation>(Func<IDependencyResolver, TService> instanceCreator) where TService : class where TImplementation : class, TService
		{
			_builder
				.Register<TService>(context => instanceCreator(new ComponentContextAdapter(context.Resolve<IComponentContext>())))
				.As<TService>()
				.SingleInstance();
			return this;
		}

		public IDependencyRegister AddSingleton<TService, TImplementation>() where TService : class where TImplementation : class, TService
		{
			_builder
				.RegisterType<TImplementation>()
				.As<TService>()
				.SingleInstance();
			return this;
		}

		public IDependencyRegister AddTransient(Type type, Func<IDependencyResolver, object> instanceCreator)
		{
			_builder.RegisterGeneric((ctxt, types, parameters) =>
			{
				return instanceCreator(new ComponentContextAdapter(ctxt));
			}).As(type).InstancePerDependency();
			return this;
		}

		public IDependencyRegister AddSingleton(Type type, Func<IDependencyResolver, object> instanceCreator)
		{
			_builder.RegisterGeneric((ctxt, types, parameters) =>
			{
				return instanceCreator(new ComponentContextAdapter(ctxt));
			}).As(type).SingleInstance();
			return this;
		}
	}
}
