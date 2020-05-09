using Ninject;
using Ninject.Modules;
using ZyRabbit.DependencyInjection;
using ZyRabbit.Instantiation;

namespace ZyRabbit.DependencyInjection.Ninject
{
	public class ZyRabbitModule : NinjectModule
	{
		public override void Load()
		{
			Kernel
				.Bind<IDependencyResolver>()
				.ToMethod(context => new NinjectAdapter(context));

			Kernel
				.Bind<IInstanceFactory>()
				.ToMethod(context => ZyRabbitFactory.CreateInstanceFactory(context.Kernel.Get<ZyRabbitOptions>()))
				.InSingletonScope();

			Kernel
				.Bind<IBusClient>()
				.ToMethod(context => context.Kernel.Get<IInstanceFactory>().Create());
		}
	}
}
