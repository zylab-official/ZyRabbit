using System;
using ZyRabbit.Configuration;
using ZyRabbit.DependencyInjection;

namespace ZyRabbit.Instantiation
{
	public class ZyRabbitOptions
	{
		public ZyRabbitConfiguration ClientConfiguration { get; set; }
		public Action<IDependencyRegister> DependencyInjection { get; set; }
		public Action<IClientBuilder> Plugins { get; set; }
	}
}
