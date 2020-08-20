using System;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace LeanCode.Components.Startup
{
    public static class IWebHostBuilderExtensions
    {
        public const string SystemLoggersEntryName = LeanProgram.SystemLoggersEntryName;

        public const string MinimumLogLevelKey = "Logging:MinimumLevel";
        public const string EnableDetailedInternalLogsKey = "Logging:EnableDetailedInternalLogs";

        public static IWebHostBuilder ConfigureOnNonDevelopmentEnvironment(
            this IWebHostBuilder builder,
            Func<IConfigurationBuilder, IConfigurationBuilder> configuration)
        {
            return builder.ConfigureAppConfiguration((context, builder) =>
            {
                if (!context.HostingEnvironment.IsDevelopment())
                {
                    configuration(builder);
                }
            });
        }

        public static IWebHostBuilder ConfigureDefaultLogging(
            this IWebHostBuilder builder,
            string projectName,
            TypesCatalog? destructurers = null,
            Action<WebHostBuilderContext, LoggerConfiguration>? additionalLoggingConfiguration = null)
        {
            var entryAssembly = Assembly.GetEntryAssembly()!; // returns null only when called from unmanaged code

            var appName = entryAssembly.GetName().Name
                ?? throw new InvalidOperationException("Failed to read entry assembly's simple name.");

            return builder.ConfigureDefaultLogging(projectName, appName, destructurers, additionalLoggingConfiguration);
        }

        public static IWebHostBuilder ConfigureDefaultLogging(
            this IWebHostBuilder builder,
            string projectName,
            string appName,
            TypesCatalog? destructurers = null,
            Action<WebHostBuilderContext, LoggerConfiguration>? additionalLoggingConfiguration = null)
        {
            return builder.ConfigureLogging((context, logging) =>
            {
                var configuration = context.Configuration;

                var loggerConfiguration = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("project", projectName)
                    .Enrich.WithProperty("app_name", appName)
                    .MinimumLevel.Is(configuration.GetValue(MinimumLogLevelKey, LogEventLevel.Verbose))
                    .DestructureCommonObjects(destructurers?.Assemblies);

                if (!configuration.GetValue<bool>(EnableDetailedInternalLogsKey))
                {
                    loggerConfiguration
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("System", LogEventLevel.Warning);
                }

                if (context.HostingEnvironment.IsDevelopment())
                {
                    loggerConfiguration
                        .WriteTo.Console()
                        .WriteTo.Seq("http://seq");
                }
                else
                {
                    loggerConfiguration
                        .WriteTo.Console(new RenderedCompactJsonFormatter());
                }

                additionalLoggingConfiguration?.Invoke(context, loggerConfiguration);

                Log.Logger = loggerConfiguration.CreateLogger();

                logging.AddConfiguration(configuration.GetSection(SystemLoggersEntryName));
                logging.AddSerilog();
            });
        }
    }
}
