using System;

namespace ZyRabbit.DependencyInjection
{
	public enum Lifetime
	{
		Singelton,
		Transient
	}

	public interface IDependencyRegister
	{
		IDependencyRegister Register(Type type, Type implementationType, Lifetime lifetime);
		IDependencyRegister Register<T>(Type type, Func<IDependencyResolver, T> instanceCreator, Lifetime lifetime) where T : class;
		bool IsRegistered(Type type);
	}

	public static class DependencyRegisterExtensions
	{
		public static IDependencyRegister AddTransient<TService, TImplementation>(this IDependencyRegister register, Func<IDependencyResolver, TImplementation> instanceCreator)
			where TService : class where TImplementation : class, TService
		{
			register.Register(typeof(TService), instanceCreator, Lifetime.Transient);
			return register;
		}

		public static IDependencyRegister AddTransient<TService, TImplementation>(this IDependencyRegister register)
			where TImplementation : class, TService where TService : class
		{
			register.Register(typeof(TService), typeof(TImplementation), Lifetime.Transient);
			return register;
		}

		public static IDependencyRegister AddSingleton(this IDependencyRegister register, Type type, Type implementationType)
		{
			register.Register(type, implementationType, Lifetime.Singelton);
			return register;
		}

		public static IDependencyRegister AddSingleton<TService>(this IDependencyRegister register, TService instance)
			where TService : class
		{
			register.Register(typeof(TService), _ => instance, Lifetime.Singelton);
			return register;
		}

		public static IDependencyRegister AddSingleton<TService, TImplementation>(this IDependencyRegister register, Func<IDependencyResolver, TService> instanceCreator)
			where TImplementation : class, TService where TService : class
		{
			register.Register(typeof(TService), instanceCreator, Lifetime.Singelton);
			return register;
		}

		public static IDependencyRegister AddSingleton<TService, TImplementation>(this IDependencyRegister register)
			where TImplementation : class, TService where TService : class
		{
			register.Register(typeof(TService), typeof(TImplementation), Lifetime.Singelton);
			return register;
		}

		public static IDependencyRegister AddTransient<TImplementation>(this IDependencyRegister register, Func<IDependencyResolver, TImplementation> instanceCreator)
			where TImplementation : class
		{
			return register.AddTransient<TImplementation, TImplementation>(instanceCreator);
		}

		public static IDependencyRegister AddSingleton<TImplementation>(this IDependencyRegister register, Func<IDependencyResolver, TImplementation> instanceCreator)
			where TImplementation : class
		{
			return register.AddSingleton<TImplementation, TImplementation>(instanceCreator);
		}
	}
}
