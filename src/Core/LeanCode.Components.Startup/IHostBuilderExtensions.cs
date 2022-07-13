using System.Reflection;
using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using LeanCode.AzureIdentity;
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

        public const string VaultUrlKey = "KeyVault:VaultUrl";
        public const string MinimumLogLevelKey = "Logging:MinimumLevel";
        public const string EnableDetailedInternalLogsKey = "Logging:EnableDetailedInternalLogs";
        public const string SeqEndpointKey = "Logging:SeqEndpoint";

        public const LogEventLevel InternalDefaultLogLevel = LogEventLevel.Warning;

        public static IHostBuilder AddAppConfigurationFromAzureKeyVault(
            this IHostBuilder builder,
            TokenCredential? credential = null,
            string? keyVaultKeyOverride = null,
            KeyVaultSecretManager? manager = null)
        {
            return builder.ConfigureAppConfiguration((context, builder) =>
            {
                ConfigureAzureKeyVault(builder, credential, keyVaultKeyOverride, manager);
            });
        }

        public static IHostBuilder AddAppConfigurationFromAzureKeyVaultOnNonDevelopmentEnvironment(
            this IHostBuilder builder,
            TokenCredential? credential = null,
            string? keyVaultKeyOverride = null,
            KeyVaultSecretManager? manager = null)
        {
            return builder.ConfigureAppConfiguration((context, builder) =>
            {
                if (!context.HostingEnvironment.IsDevelopment())
                {
                    ConfigureAzureKeyVault(builder, credential, keyVaultKeyOverride, manager);
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

                if (configuration.GetValue<string>(SeqEndpointKey) is string seqEndpoint)
                {
                    loggerConfiguration
                        .WriteTo.Seq(seqEndpoint);
                }

                if (context.HostingEnvironment.IsDevelopment())
                {
                    loggerConfiguration
                        .WriteTo.Console();
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

        private static void ConfigureAzureKeyVault(
            IConfigurationBuilder builder,
            TokenCredential? credential,
            string? keyVaultUrlKeyOverride,
            KeyVaultSecretManager? manager)
        {
            var configuration = builder.Build();

            var vault = configuration.GetValue<string?>(keyVaultUrlKeyOverride ?? VaultUrlKey);
            if (vault != null)
            {
                var vaultUrl = new Uri(vault);
                credential ??= DefaultLeanCodeCredential.Create(configuration);
                if (manager is not null)
                {
                    builder.AddAzureKeyVault(vaultUrl, credential, manager);
                }
                else
                {
                    builder.AddAzureKeyVault(vaultUrl, credential);
                }
            }
            else
            {
                throw new ArgumentException("Application startup exception: null key vault address.");
            }
        }
    }
}
