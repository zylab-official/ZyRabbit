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
using ZyRabbit.DependencyInjection;

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
	}
}
