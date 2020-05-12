using ZyRabbit.Compatibility.Legacy.Configuration;
using ZyRabbit.Enrichers.MessageContext.Context;

namespace ZyRabbit.Compatibility.Legacy
{
	public class BusClient : BusClient<MessageContext>, IBusClient
	{
		public BusClient(ZyRabbit.IBusClient client, IConfigurationEvaluator configEval) : base(client, configEval)
		{
		}
	}
}
