using System;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
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

        public const string VaultKey = "Secrets:KeyVault:VaultUrl";
        public const string ClientIdKey = "Secrets:KeyVault:ClientId";
        public const string ClientSecretKey = "Secrets:KeyVault:ClientSecret";
        public const string MinimumLogLevelKey = "Logging:MinimumLevel";
        public const string EnableDetailedInternalLogsKey = "Logging:EnableDetailedInternalLogs";

        public static IWebHostBuilder AddAppConfigurationFromAzureKeyVault(this IWebHostBuilder builder)
        {
            return builder.ConfigureAppConfiguration((context, builder) =>
            {
                var configuration = builder.Build();

                var vault = configuration.GetValue<string?>(VaultKey);
                var clientId = configuration.GetValue<string?>(ClientIdKey);
                var clientSecret = configuration.GetValue<string?>(ClientSecretKey);

                if (vault != null && clientId != null && clientSecret != null)
                {
                    builder.AddAzureKeyVault(vault, clientId, clientSecret);
                }
            });
        }

        public static IWebHostBuilder AddAppConfigurationFromAzureKeyVault(
            this IWebHostBuilder builder,
            Func<IConfiguration, AzureKeyVaultConfigurationOptions?> optionsProvider)
        {
            return builder.ConfigureAppConfiguration((context, builder) =>
            {
                var options = optionsProvider(builder.Build());

                if (options != null)
                {
                    builder.AddAzureKeyVault(options);
                }
            });
        }

        public static IWebHostBuilder ConfigureDefaultLogging(
            this IWebHostBuilder builder,
            string projectName,
            TypesCatalog? destructurers = null,
            Action<LoggerConfiguration>? additionalLoggingConfiguration = null)
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
            Action<LoggerConfiguration>? additionalLoggingConfiguration = null)
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

                additionalLoggingConfiguration?.Invoke(loggerConfiguration);

                Log.Logger = loggerConfiguration.CreateLogger();

                logging.AddConfiguration(configuration.GetSection(SystemLoggersEntryName));
                logging.AddSerilog();
            });
        }
    }
}
