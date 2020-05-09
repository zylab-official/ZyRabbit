using Autofac;
using Autofac.Features.ResolveAnything;
using ZyRabbit.DependencyInjection;
using ZyRabbit.Instantiation;

namespace ZyRabbit.DependencyInjection.Autofac
{
	public static class ContainerBuilderExtension
	{
		private const string ZyRabbit = "ZyRabbit";

		public static ContainerBuilder RegisterZyRabbit(this ContainerBuilder builder, ZyRabbitOptions options = null)
		{
			builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource(type => type.Namespace.StartsWith(ZyRabbit)));
			var adapter = new ContainerBuilderAdapter(builder);
			adapter.AddZyRabbit(options);
			return builder;
		}
	}
}
