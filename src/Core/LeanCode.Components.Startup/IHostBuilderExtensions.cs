using System;
using System.Reflection;
using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace LeanCode.Components.Startup
{
    public static class IHostBuilderExtensions
    {
        public const string SystemLoggersEntryName = LeanProgram.SystemLoggersEntryName;

        public const string VaultKey = "Secrets:KeyVault:VaultUrl";
        public const string ClientIdKey = "Secrets:KeyVault:ClientId";
        public const string ClientSecretKey = "Secrets:KeyVault:ClientSecret";
        public const string TenantIdKey = "Secrets:KeyVault:TenantId";
        public const string MinimumLogLevelKey = "Logging:MinimumLevel";
        public const string EnableDetailedInternalLogsKey = "Logging:EnableDetailedInternalLogs";

        public const LogEventLevel InternalDefaultLogLevel = LogEventLevel.Warning;

        public static IHostBuilder AddAppConfigurationFromAzureKeyVault(this IHostBuilder builder)
        {
            return builder.ConfigureAppConfiguration((context, builder) =>
            {
                ConfigureAzureKeyVault(builder);
            });
        }

        public static IHostBuilder AddAppConfigurationFromAzureKeyVaultOnNonDevelopmentEnvironment(
            this IHostBuilder builder)
        {
            return builder.ConfigureAppConfiguration((context, builder) =>
            {
                if (!context.HostingEnvironment.IsDevelopment())
                {
                    ConfigureAzureKeyVault(builder);
                }
            });
        }

        public static IHostBuilder ConfigureDefaultLogging(
            this IHostBuilder builder,
            string projectName,
            TypesCatalog? destructurers = null,
            Action<HostBuilderContext, LoggerConfiguration>? additionalLoggingConfiguration = null)
        {
            var entryAssembly = Assembly.GetEntryAssembly()!; // returns null only when called from unmanaged code

            var appName = entryAssembly.GetName().Name
                ?? throw new InvalidOperationException("Failed to read entry assembly's simple name.");

            return builder.ConfigureDefaultLogging(projectName, appName, destructurers, additionalLoggingConfiguration);
        }

        public static IHostBuilder ConfigureDefaultLogging(
            this IHostBuilder builder,
            string projectName,
            string appName,
            TypesCatalog? destructurers = null,
            Action<HostBuilderContext, LoggerConfiguration>? additionalLoggingConfiguration = null)
        {
            return builder.ConfigureLogging((context, logging) =>
            {
                var configuration = context.Configuration;
                var minLogLevel = configuration.GetValue(MinimumLogLevelKey, LogEventLevel.Verbose);

                var loggerConfiguration = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("project", projectName)
                    .Enrich.WithProperty("app_name", appName)
                    .MinimumLevel.Is(minLogLevel)
                    .DestructureCommonObjects(destructurers?.Assemblies);

                if (!configuration.GetValue<bool>(EnableDetailedInternalLogsKey))
                {
                    var internalLogLevel = minLogLevel > InternalDefaultLogLevel ? minLogLevel : InternalDefaultLogLevel;
                    loggerConfiguration
                        .MinimumLevel.Override("Microsoft", internalLogLevel)
                        .MinimumLevel.Override("System", internalLogLevel);
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

        private static void ConfigureAzureKeyVault(IConfigurationBuilder builder)
        {
            var configuration = builder.Build();

            var vault = configuration.GetValue<string?>(VaultKey);
            var tenantId = configuration.GetValue<string?>(TenantIdKey);
            var clientId = configuration.GetValue<string?>(ClientIdKey);
            var clientSecret = configuration.GetValue<string?>(ClientSecretKey);

            if (vault != null && tenantId != null && clientId != null && clientSecret != null)
            {
                var vaultUrl = new Uri(vault);
                var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                builder.AddAzureKeyVault(vaultUrl, clientSecretCredential);
            }
            else
            {
                throw new ApplicationException("Application startup exception: null key vault credentials.");
            }
        }
    }
}
