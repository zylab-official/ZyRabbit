using System;
using ZyRabbit.DependencyInjection;

namespace ZyRabbit.Instantiation
{
	public class ZyRabbitFactory
	{
		public static Disposable.BusClient CreateSingleton(ZyRabbitOptions options = null)
		{
			var ioc = new SimpleDependencyInjection();
			return CreateSingleton(options, ioc, register => ioc);
		}

		public static Disposable.BusClient CreateSingleton(ZyRabbitOptions options, IDependencyRegister register, Func<IDependencyRegister, IDependencyResolver> resolverFunc)
		{
			var factory = CreateInstanceFactory(options, register, resolverFunc);
			return new Disposable.BusClient(factory);
		}

		public static InstanceFactory CreateInstanceFactory(ZyRabbitOptions options = null)
		{
			var ioc = new SimpleDependencyInjection();
			return CreateInstanceFactory(options, ioc, register => ioc);
		}

		public static InstanceFactory CreateInstanceFactory(ZyRabbitOptions options, IDependencyRegister register, Func<IDependencyRegister, IDependencyResolver> resolverFunc)
		{
			register.AddZyRabbit(options);
			var resolver = resolverFunc(register);
			return new InstanceFactory(resolver);
		}
	}
}
