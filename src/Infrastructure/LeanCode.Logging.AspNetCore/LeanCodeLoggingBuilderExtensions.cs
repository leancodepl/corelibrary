using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace LeanCode.Logging.AspNetCore;

public static class LeanCodeLoggingBuilderExtensions
{
    public static Serilog.ILogger AddLeanCodeLogging(
        this ILoggingBuilder logging,
        IConfiguration configuration,
        string projectName,
        string appName,
        bool formatConsoleLogsAsJson,
        Assembly[]? destructurers = null,
        Action<LoggerConfiguration>? additionalLoggingConfiguration = null
    )
    {
        var minLogLevel = configuration.GetValue(IHostBuilderExtensions.MinimumLogLevelKey, LogEventLevel.Verbose);

        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("project", projectName)
            .Enrich.WithProperty("app_name", appName)
            .MinimumLevel.Is(minLogLevel)
            .DestructureCommonObjects(destructurers);

        if (!configuration.GetValue<bool>(IHostBuilderExtensions.EnableDetailedInternalLogsKey))
        {
            var internalLogLevel =
                minLogLevel > IHostBuilderExtensions.InternalDefaultLogLevel
                    ? minLogLevel
                    : IHostBuilderExtensions.InternalDefaultLogLevel;
            loggerConfiguration
                .MinimumLevel.Override("Microsoft", internalLogLevel)
                .MinimumLevel.Override("System", internalLogLevel)
                .MinimumLevel.Override("Azure.Identity", internalLogLevel)
                .MinimumLevel.Override("Azure.Core", internalLogLevel)
                .FilterOutSqlLogsWithOutboxOrInboxTables(internalLogLevel);
        }

        if (configuration.GetValue<string>(IHostBuilderExtensions.SeqEndpointKey) is string seqEndpoint)
        {
            loggerConfiguration.WriteTo.Seq(seqEndpoint, formatProvider: CultureInfo.InvariantCulture);
        }

        if (formatConsoleLogsAsJson)
        {
            loggerConfiguration.WriteTo.Console(new RenderedCompactJsonFormatter());
        }
        else
        {
            loggerConfiguration.WriteTo.Console(formatProvider: CultureInfo.InvariantCulture);
        }

        additionalLoggingConfiguration?.Invoke(loggerConfiguration);

        var logger = loggerConfiguration.CreateLogger();

        logging.Services.AddSingleton<Serilog.ILogger>(logger);
        logging.AddContextualLeanCodeLogger();

        logging.AddConfiguration(configuration.GetSection(IHostBuilderExtensions.SystemLoggersEntryName));
        logging.AddSerilog(logger, dispose: true);

        return logger;
    }
}
