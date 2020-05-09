using ZyRabbit.Instantiation;
using ZyRabbit.Serialization;

namespace ZyRabbit.Enrichers.ZeroFormatter
{
	public static class ZeroFormatterPlugin
	{
		/// <summary>
		/// Replaces the default serializer with ZeroFormatter.
		/// </summary>
		public static IClientBuilder UseZeroFormatter(this IClientBuilder builder)
		{
			builder.Register(
				pipe: p => { },
				ioc: di => di.AddSingleton<ISerializer, ZeroFormatterSerializerWorker>());
			return builder;
		}
	}
}
