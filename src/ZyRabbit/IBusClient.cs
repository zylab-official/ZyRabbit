using System;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Pipe;

namespace ZyRabbit
{
	public interface IBusClient
	{
		Task<IPipeContext> InvokeAsync(Action<IPipeBuilder> pipeCfg, Action<IPipeContext> contextCfg = null, CancellationToken token = default(CancellationToken));
	}
}
