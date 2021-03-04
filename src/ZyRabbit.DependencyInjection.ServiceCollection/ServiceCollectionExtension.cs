using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using ZyRabbit.Instantiation;
using ZyRabbit.Pipe.Middleware;

namespace ZyRabbit.DependencyInjection.ServiceCollection
{
	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddZyRabbit(this IServiceCollection collection, ZyRabbitOptions options = null)
		{
			var adapter = new ServiceCollectionAdapter(collection ?? throw new ArgumentNullException(nameof(collection)));
			adapter.AddZyRabbit(options);
			var middlewares = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Where(t => t.IsClass && !t.IsAbstract && typeof(Middleware).IsAssignableFrom(t))
				.ToArray();
			foreach (var middleware in middlewares)
			{
				collection.AddTransient(middleware);
			}

			return collection;
		}
	}
}
