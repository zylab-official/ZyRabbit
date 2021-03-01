using ZyRabbit.DependencyInjection;
using ZyRabbit.Enrichers.Protobuf;
using ZyRabbit.Instantiation;
using ZyRabbit.Serialization;

namespace ZyRabbit
{
	public static class ProtobufPlugin
	{
		/// <summary>
		/// Replaces the default serializer with Protobuf.
		/// </summary>
		public static IClientBuilder UseProtobuf(this IClientBuilder builder)
		{
			builder.Register(
				pipe: p => {},
				ioc: di => di.AddSingleton<ISerializer, ProtobufSerializer>());
			return builder;
		}
	}
}
