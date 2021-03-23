using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ZyRabbit.Configuration;
using ZyRabbit.Enrichers.GlobalExecutionId;
using ZyRabbit.Enrichers.MessageContext;
using ZyRabbit.Enrichers.MessageContext.Context;
using ZyRabbit.Instantiation;
using ZyRabbit.Messages.Sample;
using Serilog;
using Microsoft.Extensions.Logging;
using System;
using ZyRabbit.DependencyInjection;

namespace ZyRabbit.ConsoleApp.Sample
{
	public class Program
	{
		private static IBusClient _client;

		public static async Task Main()
		{
			Log.Logger = new LoggerConfiguration()
				.WriteTo.Console()
				.CreateLogger();

			_client = ZyRabbitFactory.CreateSingleton(new ZyRabbitOptions
			{
				ClientConfiguration = new ConfigurationBuilder()
					.AddJsonFile("zyrabbit.json")
					.Build()
					.Get<ZyRabbitConfiguration>(),
				Plugins = p => p
					.UseGlobalExecutionId()
					.UseMessageContext<MessageContext>(),
				DependencyInjection = register =>
				{
					register.AddSingleton(typeof(ILogger<>), typeof(SerilogLogger<>));
					register.AddSingleton<Microsoft.Extensions.Logging.ILogger, SerilogLogger<object>>(resolver => new SerilogLogger<object>());
				}
			});

			await _client.SubscribeAsync<ValuesRequested, MessageContext>((requested, ctx) => ServerValuesAsync(requested, ctx));
			await _client.RespondAsync<ValueRequest, ValueResponse>(request => SendValuesThoughRpcAsync(request));

			Console.ReadKey();
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

		private sealed class SerilogLogger<T> : ILogger<T>, Microsoft.Extensions.Logging.ILogger
		{
			private class Disposable : IDisposable
			{
				public static readonly Disposable Null = new Disposable();
				public void Dispose() {	}
			}

			public IDisposable BeginScope<TState>(TState state) => Disposable.Null;

			public bool IsEnabled(LogLevel logLevel)
			{
				return Serilog.Log.Logger.IsEnabled((Serilog.Events.LogEventLevel)logLevel);
			}

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
			{
				Serilog.Log.Logger.Write((Serilog.Events.LogEventLevel)logLevel, formatter(state, exception));
			}
		}
	}
}
