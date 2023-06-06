using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace LeanCode.Logging;

public static class IHostBuilderExtensions
{
    public const string SystemLoggersEntryName = "Serilog:SystemLoggers";
    public const string MinimumLogLevelKey = "Logging:MinimumLevel";
    public const string EnableDetailedInternalLogsKey = "Logging:EnableDetailedInternalLogs";
    public const string SeqEndpointKey = "Logging:SeqEndpoint";

    public const LogEventLevel InternalDefaultLogLevel = LogEventLevel.Warning;

    public static IHostBuilder ConfigureDefaultLogging(
        this IHostBuilder builder,
        string projectName,
        Assembly[]? destructurers = null,
        Action<HostBuilderContext, LoggerConfiguration>? additionalLoggingConfiguration = null
    )
    {
        var entryAssembly = Assembly.GetEntryAssembly()!; // returns null only when called from unmanaged code

        var appName =
            entryAssembly.GetName().Name
            ?? throw new InvalidOperationException("Failed to read entry assembly's simple name.");

        return builder.ConfigureDefaultLogging(projectName, appName, destructurers, additionalLoggingConfiguration);
    }

    public static IHostBuilder ConfigureDefaultLogging(
        this IHostBuilder builder,
        string projectName,
        string appName,
        Assembly[]? destructurers = null,
        Action<HostBuilderContext, LoggerConfiguration>? additionalLoggingConfiguration = null
    )
    {
        return builder.ConfigureLogging(
            (context, logging) =>
            {
                var configuration = context.Configuration;
                var minLogLevel = configuration.GetValue(MinimumLogLevelKey, LogEventLevel.Verbose);

                var loggerConfiguration = new LoggerConfiguration().ReadFrom
                    .Configuration(configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("project", projectName)
                    .Enrich.WithProperty("app_name", appName)
                    .MinimumLevel.Is(minLogLevel)
                    .DestructureCommonObjects(destructurers);

                if (!configuration.GetValue<bool>(EnableDetailedInternalLogsKey))
                {
                    var internalLogLevel =
                        minLogLevel > InternalDefaultLogLevel ? minLogLevel : InternalDefaultLogLevel;
                    loggerConfiguration.MinimumLevel
                        .Override("Microsoft", internalLogLevel)
                        .MinimumLevel.Override("System", internalLogLevel);
                }

                if (configuration.GetValue<string>(SeqEndpointKey) is string seqEndpoint)
                {
                    loggerConfiguration.WriteTo.Seq(seqEndpoint);
                }

                if (context.HostingEnvironment.IsDevelopment())
                {
                    loggerConfiguration.WriteTo.Console(formatProvider: CultureInfo.InvariantCulture);
                }
                else
                {
                    loggerConfiguration.WriteTo.Console(new RenderedCompactJsonFormatter());
                }

                additionalLoggingConfiguration?.Invoke(context, loggerConfiguration);

                Log.Logger = loggerConfiguration.CreateLogger();

                logging.AddConfiguration(configuration.GetSection(SystemLoggersEntryName));
                logging.AddSerilog();
            }
        );
    }
}
