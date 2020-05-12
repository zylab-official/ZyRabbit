using Ninject;
using ZyRabbit.Instantiation;

namespace ZyRabbit.DependencyInjection.Ninject
{
	public static class KernelExtension
	{
		public static IKernel RegisterZyRabbit(this IKernel config, ZyRabbitOptions options = null)
		{
			if (options != null)
			{
				config.Bind<ZyRabbitOptions>().ToConstant(options);
			}
			config.Load<ZyRabbitModule>();
			return config;
		}
	}
}
