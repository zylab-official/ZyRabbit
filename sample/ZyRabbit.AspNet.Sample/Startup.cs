using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ZyRabbit.AspNet.Sample.Controllers;
using ZyRabbit.Configuration;
using ZyRabbit.DependencyInjection.ServiceCollection;
using ZyRabbit.Enrichers.GlobalExecutionId;
using ZyRabbit.Enrichers.HttpContext;
using ZyRabbit.Enrichers.MessageContext;
using ZyRabbit.Instantiation;

namespace ZyRabbit.AspNet.Sample
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

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
						.UseMessageContext(c => new MessageContext
						{
							Source = c.GetHttpContext().Request.GetDisplayUrl()
						})
				})
				.AddControllers();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();
			app.UseRouting();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}

		private ZyRabbitConfiguration GetZyRabbitConfiguration()
		{
			var section = Configuration.GetSection("ZyRabbit");
			if (!section.GetChildren().Any())
			{
				throw new ArgumentException("Unable to configuration section 'ZyRabbit'. Make sure it exists in the provided configuration");
			}
			return section.Get<ZyRabbitConfiguration>();
		}
	}
}
