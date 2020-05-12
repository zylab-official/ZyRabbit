using Microsoft.Extensions.DependencyInjection;
using ZyRabbit.Instantiation;

namespace ZyRabbit.DependencyInjection.ServiceCollection
{
	public static class AddZyRabbitExtension
	{
		public static IServiceCollection AddZyRabbit(this IServiceCollection collection, ZyRabbitOptions options = null)
		{
			var adapter = new ServiceCollectionAdapter(collection);
			adapter.AddZyRabbit(options);
			options?.DependencyInjection?.Invoke(adapter);
			return collection;
		}
	}
}
