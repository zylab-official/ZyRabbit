using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ZyRabbit.DependencyInjection.ServiceCollection
{
	public class ServiceProviderAdapter : IDependencyResolver
	{
		private readonly IServiceProvider _provider;

		public ServiceProviderAdapter(IServiceProvider provider)
		{
			_provider = provider ?? throw new ArgumentNullException(nameof(provider));
		}

		public object GetService(Type serviceType, params object[] additional)
		{
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));

			additional = additional ?? Array.Empty<object>();
			if (additional.Length > 0)
			{
				var additionalTypes = additional.Select(a => a.GetType()).ToArray();
				var additionalParams = new LinkedList<object>(additional);

				var ctor = serviceType.GetConstructors().FirstOrDefault();
				if (ctor == null)
				{
					throw new Exception($"Unable to find suitable constructor for {serviceType.Name}.");
				}

				var parameters = ctor.GetParameters().Select((p, i) =>
				{
					if (additionalTypes.Contains(p.ParameterType))
					{
						for (var parameter = additionalParams.First; parameter != null;)
						{
							var next = parameter.Next;
							if (parameter.Value.GetType() == p.ParameterType)
							{
								additionalParams.Remove(parameter.Value);
								return parameter.Value;
							}

							parameter = next;
						}

						if (p.Attributes.HasFlag(ParameterAttributes.Optional))
						{
							return p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null;
						}
						else
						{
							throw new Exception($"Cannot resolve parameter of type {p.ParameterType} in constructor {ctor} for type '{serviceType.Name}'.");
						}
					}
					else
					{
						if (p.Attributes.HasFlag(ParameterAttributes.Optional))
						{
							return _provider.GetService(p.ParameterType);
						}

						return _provider.GetRequiredService(p.ParameterType);
					}
				}).ToArray();

				return ctor.Invoke(parameters);
			}

			return _provider.GetService(serviceType) ?? ActivatorUtilities.CreateInstance(_provider, serviceType);
		}
	}
}
