using RabbitMQ.Client;
using ZyRabbit.Configuration.Get;
using ZyRabbit.Pipe;

namespace ZyRabbit.Operations.Get
{

	public static class GetPipeExtensions
	{
		public const string GetConfiguration = "GetConfiguration";
		public const string BasicGetResult = "BasicGetResult";

		public static GetConfiguration GetGetConfiguration(this IPipeContext context)
		{
			return context.Get<GetConfiguration>(GetConfiguration);
		}

		public static BasicGetResult GetBasicGetResult(this IPipeContext context)
		{
			return context.Get<BasicGetResult>(BasicGetResult);
		}
	}
}
