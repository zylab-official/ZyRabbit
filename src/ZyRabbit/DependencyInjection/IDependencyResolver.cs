using System;

namespace ZyRabbit.DependencyInjection
{
	public interface IDependencyResolver
	{
		object GetService(Type serviceType, params object[] additional);
	}

	public static class DependencyResolverExtensions
	{
		public static TService GetService<TService>(this IDependencyResolver resolver, params object[] additional)
		{
			if (resolver == null)
				throw new ArgumentNullException(nameof(resolver));

			return (TService)resolver.GetService(typeof(TService), additional);
		}
	}	
}
