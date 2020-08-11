using System;
using System.Threading.Tasks;
using BlazorServerHost.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace BlazorServerHost
{
	public class Program
	{
		public static async Task<int> Main(string[] args)
		{
			ILogger logger = null;

			try
			{
				var host = CreateHostBuilder(args)
						.Build();

				logger = host.Services.GetRequiredService<ILogger<Program>>();

				// try to migrate database
				using var scope = host.Services.CreateScope();
				var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
				await context.Database.MigrateAsync();

				await host.RunAsync();
				return 0;
			}
			catch (Exception ex)
			{
				logger?.LogCritical(ex, "A exception caused the service to crash.");
				return 1;
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseSerilog((context, configuration) =>
				{
					configuration
						.Enrich.FromLogContext()
						.Enrich.WithProperty("Application", "BlazorServer")
						.ReadFrom.Configuration(context.Configuration);
				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
