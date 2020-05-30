using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.DAL;
using WebApplication1.Middleware;
using WebApplication1.Services;

namespace WebApplication1
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
					.AddJwtBearer(options =>
					{
						options.TokenValidationParameters = new TokenValidationParameters
						{
							ValidateIssuer = true,
							ValidateAudience = true,
							ValidateLifetime = true,
							ValidIssuer = "Gakko",
							ValidAudience = "Students",
							IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]))
						};
					});

			services.AddControllers()
					.AddXmlSerializerFormatters();
			services.AddSingleton<IDbService, MockDbService>();
			services.AddTransient<IStudentsDbService, SqlServerDbService>();
			services.AddControllers();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IStudentsDbService service)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseMiddleware<LoggingMiddleware>();

			if (env.IsDevelopment())
				{
					app.UseDeveloperExceptionPage();
				}

				app.UseMiddleware<LoggingMiddleware>();

				app.Use(async (context, next) =>
				{
					if (!context.Request.Headers.ContainsKey("Index"))
					{
						context.Response.StatusCode = StatusCodes.Status401Unauthorized;
						await context.Response.WriteAsync("Musi zawieraæ indeks");
						return;
					}

					string index = context.Request.Headers["Index"].ToString();
					if (!service.CheckIndex(index))
					{
						context.Response.StatusCode = StatusCodes.Status404NotFound;
						await context.Response.WriteAsync("Podany indeks nie istnieje!");
						return;
					}

					await next();
				});

				app.UseRouting();


			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
