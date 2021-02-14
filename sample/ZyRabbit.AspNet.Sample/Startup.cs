using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZyRabbit.AspNet.Sample.Controllers;
using ZyRabbit.Configuration;
using ZyRabbit.DependencyInjection.ServiceCollection;
using ZyRabbit.Enrichers.GlobalExecutionId;
using ZyRabbit.Enrichers.HttpContext;
using ZyRabbit.Enrichers.MessageContext;
using ZyRabbit.Instantiation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ZyRabbit.AspNet.Sample
{
	public class Startup
	{
		private readonly IConfiguration _configuration;

		public Startup(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services
				.AddZyRabbit(new ZyRabbitOptions
					{
						ClientConfiguration = GetZyRabbitConfiguration(),
						Plugins = p => p
							.UseStateMachine()
							.UseGlobalExecutionId()
							.UseHttpContext()
							.UseMessageContext(c =>
							{
								return new MessageContext
								{
									Source = c.GetHttpContext().Request.GetDisplayUrl()
								};
							}),
						DependencyInjection = register =>
						{
							register.AddSingleton<IServiceProvider, IServiceProvider>(_ => services.BuildServiceProvider());
							register.AddSingleton(typeof(ILogger<>), typeof(LoggerProxy<>));
							register.AddSingleton<ILogger, LoggerProxy>();
						}
					})
				.AddZyRabbit()
				.AddControllers();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}

		private ZyRabbitConfiguration GetZyRabbitConfiguration()
		{
			const string sectionName = "ZyRabbit";
			var section = _configuration.GetSection(sectionName);
			if (!section.Exists())
			{
				throw new ArgumentException($"Unable to configuration section '{sectionName}'. Make sure it exists in the provided configuration");
			}

			return section.Get<ZyRabbitConfiguration>();
		}

		private class LoggerProxy<T> : ILogger<T>
		{
			private readonly ILogger<T> _logger;

			public LoggerProxy(IServiceProvider serviceProvider)
			{
				_logger = serviceProvider.GetRequiredService<ILogger<T>>();
			}

			public IDisposable BeginScope<TState>(TState state)
			{
				return _logger.BeginScope<TState>(state);
			}

			public bool IsEnabled(LogLevel logLevel)
			{
				return _logger.IsEnabled(logLevel);
			}

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
			{
				_logger.Log(logLevel, eventId, state, exception, formatter);
			}
		}

		private class LoggerProxy : ILogger
		{
			private readonly ILogger _logger;

			public LoggerProxy(IServiceProvider serviceProvider)
			{
				_logger = serviceProvider.GetRequiredService<ILogger>();
			}

			public IDisposable BeginScope<TState>(TState state)
			{
				return _logger.BeginScope<TState>(state);
			}

			public bool IsEnabled(LogLevel logLevel)
			{
				return _logger.IsEnabled(logLevel);
			}

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
			{
				_logger.Log(logLevel, eventId, state, exception, formatter);
			}
		}
	}
}
