using System;
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

		public TService GetService<TService>(params object[] additional)
		{
			return (TService)GetService(typeof(TService), additional);
		}

		public object GetService(Type serviceType, params object[] additional)
		{
			additional = additional ?? Array.Empty<object>();
			var service = _provider.GetService(serviceType);
			return service ?? ActivatorUtilities.CreateInstance(_provider, serviceType, additional);
		}
	}
}
