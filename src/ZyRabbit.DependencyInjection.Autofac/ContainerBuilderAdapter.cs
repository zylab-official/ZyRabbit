using System;
using Autofac;

namespace ZyRabbit.DependencyInjection.Autofac
{
	public class ContainerBuilderAdapter : IDependencyRegister
	{
		private readonly ContainerBuilder _builder;

		public ContainerBuilderAdapter(ContainerBuilder builder)
		{
			_builder = builder ?? throw new ArgumentNullException(nameof(builder));
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

		public IDependencyRegister AddTransient(Type type, Func<IDependencyResolver, Type, object> instanceCreator)
		{
			_builder.RegisterGeneric((ctxt, types, parameters) =>
			{
				return instanceCreator(new ComponentContextAdapter(ctxt), type);
			}).As(type).InstancePerDependency();
			return this;
		}

		public IDependencyRegister AddSingleton(Type type, Func<IDependencyResolver, Type, object> instanceCreator)
		{
			_builder.RegisterGeneric((ctxt, types, parameters) =>
			{
				var generic = type.MakeGenericType(types);
				return instanceCreator(new ComponentContextAdapter(ctxt), generic);
			}).As(type).SingleInstance();
			return this;
		}

		public IDependencyRegister AddSingleton(Type type, Type implementationType)
		{
			if (type.IsGenericTypeDefinition)
			{
				_builder.RegisterGeneric(implementationType).As(type).SingleInstance();
			}
			else
			{
				_builder.RegisterType(implementationType).As(type).SingleInstance();

			}

			return this;
		}
	}
}
