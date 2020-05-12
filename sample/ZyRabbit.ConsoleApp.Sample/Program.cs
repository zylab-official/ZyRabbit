using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ZyRabbit.Configuration;
using ZyRabbit.Enrichers.GlobalExecutionId;
using ZyRabbit.Enrichers.MessageContext;
using ZyRabbit.Enrichers.MessageContext.Context;
using ZyRabbit.Instantiation;
using ZyRabbit.Messages.Sample;
using Serilog;

namespace ZyRabbit.ConsoleApp.Sample
{
	public class Program
	{
		private static IBusClient _client;

		public static void Main(string[] args)
		{
			RunAsync().GetAwaiter().GetResult();
		}

		public static async Task RunAsync()
		{
			Log.Logger = new LoggerConfiguration()
				.WriteTo.LiterateConsole()
				.CreateLogger();

			_client = ZyRabbitFactory.CreateSingleton(new ZyRabbitOptions
			{
				ClientConfiguration = new ConfigurationBuilder()
					.SetBasePath(Directory.GetCurrentDirectory())
					.AddJsonFile("zyrabbit.json")
					.Build()
					.Get<ZyRabbitConfiguration>(),
				Plugins = p => p
					.UseGlobalExecutionId()
					.UseMessageContext<MessageContext>()
			});

			await _client.SubscribeAsync<ValuesRequested, MessageContext>((requested, ctx) => ServerValuesAsync(requested, ctx));
			await _client.RespondAsync<ValueRequest, ValueResponse>(request => SendValuesThoughRpcAsync(request));
		}

		private static Task<ValueResponse> SendValuesThoughRpcAsync(ValueRequest request)
		{
			return Task.FromResult(new ValueResponse
			{
				Value = $"value{request.Value}"
			});
		}

		private static Task ServerValuesAsync(ValuesRequested message, MessageContext ctx)
		{
			var values = new List<string>();
			for (var i = 0; i < message.NumberOfValues; i++)
			{
				values.Add($"value{i}");
			}
			return _client.PublishAsync(new ValuesCalculated { Values = values });
		}
	}
}
