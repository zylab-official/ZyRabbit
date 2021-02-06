using Microsoft.Extensions.DependencyInjection;
using System;
using ZyRabbit.Instantiation;

namespace ZyRabbit.DependencyInjection.ServiceCollection
{
	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddZyRabbit(this IServiceCollection collection, ZyRabbitOptions options = null)
		{
			var adapter = new ServiceCollectionAdapter(collection ?? throw new ArgumentNullException(nameof(collection)));
			adapter.AddZyRabbit(options);
			options?.DependencyInjection?.Invoke(adapter);
			return collection;
		}
	}
}
