using Autofac;
using Autofac.Features.ResolveAnything;
using System;
using ZyRabbit.Instantiation;

namespace ZyRabbit.DependencyInjection.Autofac
{
	public static class ContainerBuilderExtension
	{
		private const string ZyRabbit = "ZyRabbit";

		public static ContainerBuilder RegisterZyRabbit(this ContainerBuilder builder, ZyRabbitOptions options = null)
		{
			if (builder == null)
				throw new ArgumentNullException(nameof(builder));

			var adapter = new ContainerBuilderAdapter(builder);
			adapter.AddZyRabbit(options);
			builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource(type => type.Namespace.StartsWith(ZyRabbit)));

			return builder;
		}
	}
}
