using System;

namespace ZyRabbit.DependencyInjection
{
	public interface IDependencyResolver
	{
		TService GetService<TService>(params object[] additional);
		object GetService(Type serviceType, params object[] additional);
	}
}
