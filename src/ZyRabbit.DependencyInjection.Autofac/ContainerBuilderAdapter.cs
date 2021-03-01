using System;
using Autofac;
using Autofac.Core;

namespace ZyRabbit.DependencyInjection.Autofac
{
	public class ContainerBuilderAdapter : IDependencyRegister
	{
		private readonly ContainerBuilder _builder;

		public ContainerBuilderAdapter(ContainerBuilder builder)
		{
			_builder = builder ?? throw new ArgumentNullException(nameof(builder));
		}

		public IDependencyRegister Register(Type type, Type implementationType, Lifetime lifetime)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (implementationType == null)
				throw new ArgumentNullException(nameof(implementationType));

			if (type.IsGenericTypeDefinition)
			{
				var binding = _builder.RegisterGeneric(implementationType).As(type);
				switch (lifetime)
				{
					case Lifetime.Transient:
						{
							binding.InstancePerDependency();
							break;
						}
					case Lifetime.Singelton:
						{
							binding.SingleInstance();
							break;
						}
					default:
						throw new NotSupportedException($"Lifetime '{lifetime}' is not supported");
				}
			}
			else
			{
				var binding = _builder.RegisterType(implementationType).As(type);
				switch (lifetime)
				{
					case Lifetime.Transient:
						{
							binding.InstancePerDependency();
							break;
						}
					case Lifetime.Singelton:
						{
							binding.SingleInstance();
							break;
						}
					default:
						throw new NotSupportedException($"Lifetime '{lifetime}' is not supported");
				}
			}

			return this;
		}

		public IDependencyRegister Register<T>(Type type, Func<IDependencyResolver, T> instanceCreator, Lifetime lifetime) where T : class
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (instanceCreator == null)
				throw new ArgumentNullException(nameof(instanceCreator));

			var binding = _builder
				.Register(context => instanceCreator(new ComponentContextAdapter(context.Resolve<IComponentContext>())))
				.As<T>();

			switch (lifetime)
			{
				case Lifetime.Transient:
					{
						binding.InstancePerDependency();
						break;
					}
				case Lifetime.Singelton:
					{
						binding.SingleInstance();
						break;
					}
				default:
					throw new NotSupportedException($"Lifetime '{lifetime}' is not supported");
			}

			return this;
		}

		public bool IsRegistered(Type type)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			return _builder.ComponentRegistryBuilder.IsRegistered(new TypedService(type));
		}
	}
}
