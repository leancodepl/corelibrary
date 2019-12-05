using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace LeanCode.Components.Startup
{
    public static class LeanProgram
    {
        public const string SystemLoggersEntryName = "Serilog:SystemLoggers";

        [Obsolete]
        public static IWebHostBuilder BuildMinimalWebHost<TStartup>(
            string appName,
            Func<WebHostBuilderContext, LoggerConfiguration> configLogger,
            Action<WebHostBuilderContext, IConfigurationBuilder>? extendConfig = null,
            TypesCatalog? destructurers = null)
            where TStartup : class
        {
            return new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddEnvironmentVariables();
                    extendConfig?.Invoke(hostingContext, config);
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    Log.Logger = configLogger(hostingContext)
                        .EnrichWithAppName(appName)
                        .DestructureCommonObjects(destructurers?.Assemblies)
                        .CreateLogger();

                    var config = hostingContext.Configuration
                        .GetSection(SystemLoggersEntryName);

                    logging.AddConfiguration(config);
                    logging.AddSerilog();
                })
                .UseDefaultServiceProvider((context, options) =>
                {
                    options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
                })
                .UseStartup<TStartup>();
        }

        public static IWebHostBuilder BuildMinimalWebHost<TStartup>()
            where TStartup : class
        {
            return new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddEnvironmentVariables();
                })
                .UseStartup<TStartup>();
        }
    }
}
