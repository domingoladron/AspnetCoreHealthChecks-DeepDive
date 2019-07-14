using System;
using System.IO;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace HealthChecks.SomeModelService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
            try
            {
                logger.Debug("inititalising Microservice.Restful");
                var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                var host = new WebHostBuilder()
                    .UseKestrel()
                    .ConfigureServices(services => services.AddAutofac())
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.SetBasePath(Directory.GetCurrentDirectory());
                        config.AddJsonFile("appsettings.json",true,reloadOnChange: true);
                        config.AddJsonFile($"appsettings.{environmentName}.json",optional: true, reloadOnChange:true);
                    })
                    .ConfigureLogging((hostingContext, logging) =>
                    {
                        logging.ClearProviders();
                        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                        logging.AddConsole();
                        logging.AddDebug();
                        logging.SetMinimumLevel(LogLevel.Trace);
                    })
                    .UseNLog()
                    .UseStartup<Startup>()
                    .Build();

                host.Run();
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }

        }

    }
}
