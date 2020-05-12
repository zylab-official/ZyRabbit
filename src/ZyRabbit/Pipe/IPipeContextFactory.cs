using System.Collections.Concurrent;
using System.Collections.Generic;
using ZyRabbit.Configuration;

namespace ZyRabbit.Pipe
{
	public interface IPipeContextFactory
	{
		IPipeContext CreateContext(params KeyValuePair<string, object>[] additional);
	}

	public class PipeContextFactory : IPipeContextFactory
	{
		private readonly ZyRabbitConfiguration _config;

		public PipeContextFactory(ZyRabbitConfiguration config)
		{
			_config = config;
		}

		public IPipeContext CreateContext(params KeyValuePair<string, object>[] additional)
		{
			return new PipeContext
			{
				Properties = new ConcurrentDictionary<string, object>(additional)
				{
					[PipeKey.ClientConfiguration] = _config
				}
			};
		}
	}
}
