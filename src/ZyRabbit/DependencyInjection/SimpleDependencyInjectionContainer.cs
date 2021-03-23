using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ZyRabbit.DependencyInjection
{
	public class SimpleDependencyInjectionContainer : IDependencyRegister, IDependencyResolver, IDisposable
	{
		private enum SearchResult
		{
			NotFound,
			Activation,
			Factory
		}

		private readonly Dictionary<Type, Func<IDependencyResolver, Type, object>> _registrations = new Dictionary<Type, Func<IDependencyResolver, Type, object>>();
		private readonly ConcurrentDictionary<Type, object> _singletonInstances = new ConcurrentDictionary<Type, object>();

		private (Type, SearchResult) FindRegistrationKey(Type type)
		{
			if (_registrations.ContainsKey(type))
			{
				return (type, SearchResult.Factory);
			}

			var typeInfo = type.GetTypeInfo();
			if (!typeInfo.IsAbstract)
			{
				return (type, SearchResult.Activation);
			}

			if (typeInfo.IsAbstract && typeInfo.IsConstructedGenericType)
			{
				var typeDef = typeInfo.GetGenericTypeDefinition();
				if (_registrations.ContainsKey(typeDef))
				{
					return (typeDef, SearchResult.Factory);
				}
			}

			return (null, SearchResult.NotFound);
		}

		public object GetService(Type serviceType, params object[] additional)
		{
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));

			var (type, result) = FindRegistrationKey(serviceType);
			switch (result)
			{
				case SearchResult.Activation:
					return CreateInstance(type, additional);
				case SearchResult.Factory:
					return _registrations[type](this, serviceType);
				default:
					throw new InvalidOperationException("No registration for " + serviceType);
			}
		}

		private bool TryGetService(Type serviceType, out object service, params object[] additional)
		{
			var (type, result) = FindRegistrationKey(serviceType);
			switch (result)
			{
				case SearchResult.Activation:
					service = CreateInstance(type, additional);
					return true;
				case SearchResult.Factory:
					service = _registrations[type](this, serviceType);
					return true;
				default:
					service = null;
					return false;
			}
		}

		private object CreateInstance(Type implementationType, IEnumerable<object> additional)
		{
			var additionalTypes = additional.Select(a => a.GetType());
			var ctors = implementationType
				.GetConstructors();
			var ctor = ctors
				.Where(c => c.GetParameters().All(p => {
					if (p.Attributes.HasFlag(ParameterAttributes.Optional) || additionalTypes.Contains(p.ParameterType))
						return true;
					var (_, result) = FindRegistrationKey(p.ParameterType);
					return result != SearchResult.NotFound;
				}))
				.OrderByDescending(c => c.GetParameters().Length)
				.FirstOrDefault();
			if (ctor == null)
			{
				throw new Exception($"Unable to find suitable constructor for {implementationType.Name}.");
			}
			var dependencies = ctor
				.GetParameters()
				.Select(parameter =>
				{
					if (additionalTypes.Contains(parameter.ParameterType))
					{
						return additional.First(a => a.GetType() == parameter.ParameterType);
					}
					object service;
					return TryGetService(parameter.ParameterType, out service) ? service : null;
				})
				.ToArray();
			return ctor.Invoke(dependencies);
		}

		public IDependencyRegister Register(Type serviceType, Type implementationType, Lifetime lifetime)
		{
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));
			if (implementationType == null)
				throw new ArgumentNullException(nameof(implementationType));

			object CreateInstanceFromType(Type requestedType)
			{
				if (implementationType.IsGenericTypeDefinition)
				{
					var ts = implementationType.MakeGenericType(requestedType.GetGenericArguments());
					return CreateInstance(ts, Enumerable.Empty<object>());
				}

				return CreateInstance(implementationType, Enumerable.Empty<object>());
			}

			switch (lifetime)
			{
				case Lifetime.Transient:
					{
						_registrations[serviceType] = (resolver, requestedType) => CreateInstanceFromType(requestedType);
						break;
					}
				case Lifetime.Singleton:
					{
						_registrations[serviceType] = (resolver, requestedType) =>
						{
							var result = _singletonInstances.GetOrAdd(requestedType, key =>	CreateInstanceFromType(key));
							return result;
						};

						break;
					}
				default:
					throw new NotSupportedException($"Lifetime '{lifetime}' is not supported");
			}

			return this;
		}

		public IDependencyRegister Register<T>(Type serviceType, Func<IDependencyResolver, T> instanceCreator, Lifetime lifetime) where T : class
		{
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));
			if (instanceCreator == null)
				throw new ArgumentNullException(nameof(instanceCreator));

			switch (lifetime)
			{
				case Lifetime.Transient:
					{
						_registrations[serviceType] = (resolver, type) => instanceCreator(resolver);
						break;
					}
				case Lifetime.Singleton:
					{
						_registrations[serviceType] = (resolver, requestedType) =>
						{
							var result = _singletonInstances.GetOrAdd(requestedType, _ =>
							{
								return instanceCreator(resolver);
							});
							return result;
						};
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

			return _registrations.ContainsKey(type);
		}

		public void Dispose()
		{
			_singletonInstances.Clear();
			_registrations.Clear();
		}
	}
}
