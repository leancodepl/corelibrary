using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace LeanCode.Components.Startup
{
    public static class LeanProgram
    {
        public static IWebHostBuilder BuildDefaultWebHost<TStartup>(
            bool requireEnvSettings = false,
            bool requireBaseSettings = true)
            where TStartup : class
        {
            return new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    config
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", requireBaseSettings, true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", requireEnvSettings, true);

                    if (env.IsDevelopment())
                    {
                        config = config.AddUserSecrets<TStartup>();
                    }

                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    var config = hostingContext.Configuration
                        .GetSection(LeanStartup.SystemLoggersEntryName);
                    logging.AddConfiguration(config);
                    logging.AddDebug();
                    logging.AddSerilog();
                })
                .UseDefaultServiceProvider((context, options) =>
                {
                    options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
                })
                .UseStartup<TStartup>();
        }
    }
}
