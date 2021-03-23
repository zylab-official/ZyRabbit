using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZyRabbit.Common;
using ZyRabbit.Configuration.Exchange;

namespace ZyRabbit.Pipe.Middleware
{
	public class ExchangeDeclareOptions
	{
		public Func<IPipeContext, ExchangeDeclaration> ExchangeFunc { get; set; }
		public  Func<IPipeContext, bool> ThrowOnFailFunc { get; set; }
	}

	public class ExchangeDeclareMiddleware : Middleware
	{
		protected readonly ITopologyProvider TopologyProvider;
		protected readonly Func<IPipeContext, ExchangeDeclaration> ExchangeFunc;
		protected readonly Func<IPipeContext, bool> ThrowOnFailFunc;
		protected readonly ILogger<ExchangeDeclareMiddleware> Logger;

		public ExchangeDeclareMiddleware(ITopologyProvider topologyProvider, ILogger<ExchangeDeclareMiddleware> logger, ExchangeDeclareOptions options = null)
		{
			TopologyProvider = topologyProvider ?? throw new ArgumentNullException(nameof(topologyProvider));
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			ExchangeFunc = options?.ExchangeFunc ?? (context => context.GetExchangeDeclaration());
			ThrowOnFailFunc = options?.ThrowOnFailFunc ?? (context => false);
		}

		public override async Task InvokeAsync(IPipeContext context, CancellationToken token)
		{
			var exchangeCfg = GetExchangeDeclaration(context);

			if (exchangeCfg != null)
			{
				Logger.LogDebug($"Exchange configuration found. Declaring '{exchangeCfg.Name}'.");
				await DeclareExchangeAsync(exchangeCfg, context, token);
			}
			else
			{
				if (GetThrowOnFail(context))
				{
					throw new InvalidOperationException("Exchange declaration config was not found");
				}
			}

			await Next.InvokeAsync(context, token);
		}

		protected virtual ExchangeDeclaration GetExchangeDeclaration(IPipeContext context)
		{
			return ExchangeFunc?.Invoke(context);
		}

		protected virtual bool GetThrowOnFail(IPipeContext context)
		{
			return ThrowOnFailFunc(context);
		}

		protected virtual Task DeclareExchangeAsync(ExchangeDeclaration exchange, IPipeContext context, CancellationToken token)
		{
			return TopologyProvider.DeclareExchangeAsync(exchange);
		}
	}
}
