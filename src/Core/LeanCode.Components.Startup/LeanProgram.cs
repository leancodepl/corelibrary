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

        public static IWebHostBuilder BuildDefaultWebHost<TStartup>(
            string appName,
            bool requireEnvSettings = false,
            bool requireBaseSettings = true,
            TypesCatalog destructurers = null)
            where TStartup : class
        {
            return new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    config
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", !requireBaseSettings, true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", !requireEnvSettings, true);

                    if (env.IsDevelopment())
                    {
                        config = config.AddUserSecrets<TStartup>();
                    }

                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    Log.Logger = new LoggerConfiguration()
                        .EnrichWithAppName(appName)
                        .ReadFrom.Configuration(hostingContext.Configuration)
                        .DestructureCommonObjects(destructurers.Assemblies)
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

        public static IWebHostBuilder BuildMinimalWebHost<TStartup>(
            string appName,
            Func<WebHostBuilderContext, LoggerConfiguration> configLogger,
            Action<WebHostBuilderContext, IConfigurationBuilder> extendConfig = null,
            TypesCatalog destructurers = null)
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
                        .DestructureCommonObjects(destructurers.Assemblies)
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
    }
}
