using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace ZyRabbit.DependencyInjection.ServiceCollection
{
	public class ServiceCollectionAdapter : IDependencyRegister
	{
		private readonly IServiceCollection _collection;

		public ServiceCollectionAdapter(IServiceCollection collection)
		{
			_collection = collection ?? throw new ArgumentNullException(nameof(collection));
		}

		public IDependencyRegister Register(Type type, Type implementationType, Lifetime lifetime)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (implementationType == null)
				throw new ArgumentNullException(nameof(implementationType));

			switch (lifetime)
			{
				case Lifetime.Transient:
					{
						_collection.AddTransient(type, implementationType);
						break;
					}
				case Lifetime.Singelton:
					{
						_collection.AddSingleton(type, implementationType);
						break;
					}
				default:
					throw new NotSupportedException($"Lifetime '{lifetime}' is not supported");
			}

			return this;
		}

		public IDependencyRegister Register<T>(Type type, Func<IDependencyResolver, T> instanceCreator, Lifetime lifetime) where T : class
		{
			if (instanceCreator == null)
				throw new ArgumentNullException(nameof(instanceCreator));

			switch (lifetime)
			{
				case Lifetime.Transient:
					{
						_collection.Add(new ServiceDescriptor(type, c => instanceCreator(new ServiceProviderAdapter(c)), ServiceLifetime.Transient));
						break;
					}
				case Lifetime.Singelton:
					{
						_collection.Add(new ServiceDescriptor(type, c => instanceCreator(new ServiceProviderAdapter(c)), ServiceLifetime.Singleton));
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

			return _collection.Any(d => d.ServiceType == type);
		}
	}
}
