using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZyRabbit.AspNet.Sample.Controllers;
using ZyRabbit.Configuration;
using ZyRabbit.DependencyInjection.ServiceCollection;
using ZyRabbit.Enrichers.GlobalExecutionId;
using ZyRabbit.Enrichers.HttpContext;
using ZyRabbit.Enrichers.MessageContext;
using ZyRabbit.Instantiation;
using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace ZyRabbit.AspNet.Sample
{
	public class Startup
	{
		private readonly string _rootPath;

		public Startup(IHostingEnvironment env)
		{
			_rootPath = env.ContentRootPath;
			var builder = new ConfigurationBuilder()
				.SetBasePath(_rootPath)
				.AddJsonFile("appsettings.json")
				.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public IConfigurationRoot Configuration { get; }

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
							})
					})
				.AddMvc();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			Log.Logger = GetConfiguredSerilogger();
			loggerFactory
				.AddSerilog()
				.AddConsole(Configuration.GetSection("Logging"));

			app.UseMvc();
		}

		private ILogger GetConfiguredSerilogger()
		{
			return new LoggerConfiguration()
				.WriteTo.File($"{_rootPath}/Logs/serilog.log", LogEventLevel.Debug)
				.WriteTo.LiterateConsole()
				.CreateLogger();
		}

		private ZyRabbitConfiguration GetZyRabbitConfiguration()
		{
			var section = Configuration.GetSection("ZyRabbit");
			if (!section.GetChildren().Any())
			{
				throw new ArgumentException($"Unable to configuration section 'ZyRabbit'. Make sure it exists in the provided configuration");
			}
			return section.Get<ZyRabbitConfiguration>();
		}
	}
}
