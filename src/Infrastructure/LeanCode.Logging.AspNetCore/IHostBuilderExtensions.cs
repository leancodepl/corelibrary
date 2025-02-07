using System.Buffers;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Filters;

namespace LeanCode.Logging.AspNetCore;

public static class IHostBuilderExtensions
{
    public const string SystemLoggersEntryName = "Serilog:SystemLoggers";
    public const string MinimumLogLevelKey = "Logging:MinimumLevel";
    public const string EnableDetailedInternalLogsKey = "Logging:EnableDetailedInternalLogs";
    public const string SeqEndpointKey = "Logging:SeqEndpoint";

    public const LogEventLevel InternalDefaultLogLevel = LogEventLevel.Warning;

    public static readonly ImmutableArray<string> OutboxInboxTablesToFilterOut =
    [
        "InboxState",
        "OutboxState",
        "OutboxMessage",
    ];

    public static IHostBuilder ConfigureDefaultLogging(
        this IHostBuilder builder,
        string projectName,
        Assembly[]? destructurers = null,
        bool preserveStaticLogger = false,
        Action<HostBuilderContext, LoggerConfiguration>? additionalLoggingConfiguration = null
    )
    {
        var entryAssembly = Assembly.GetEntryAssembly()!; // returns null only when called from unmanaged code

        var appName =
            entryAssembly.GetName().Name
            ?? throw new InvalidOperationException("Failed to read entry assembly's simple name.");

        return builder.ConfigureDefaultLogging(
            projectName,
            appName,
            destructurers,
            preserveStaticLogger,
            additionalLoggingConfiguration
        );
    }

    public static IHostBuilder ConfigureDefaultLogging(
        this IHostBuilder builder,
        string projectName,
        string appName,
        Assembly[]? destructurers = null,
        bool preserveStaticLogger = false,
        Action<HostBuilderContext, LoggerConfiguration>? additionalLoggingConfiguration = null
    )
    {
        return builder.ConfigureLogging(
            (context, logging) =>
            {
                var configuration = context.Configuration;
                var logger = logging.AddLeanCodeLogging(
                    context.Configuration,
                    projectName,
                    appName,
                    formatConsoleLogsAsJson: !context.HostingEnvironment.IsDevelopment(),
                    destructurers: destructurers,
                    additionalLoggingConfiguration: additionalLoggingConfiguration is null
                        ? null
                        : lc => additionalLoggingConfiguration(context, lc)
                );

                if (preserveStaticLogger)
                {
                    Log.Logger = logger;
                }
            }
        );
    }

    public static LoggerConfiguration FilterOutSqlLogsWithOutboxOrInboxTables(
        this LoggerConfiguration loggerConfiguration,
        LogEventLevel logLevel,
        string[]? tablesToFilterOverride = null
    )
    {
        var fromSourcePredicate = Matching.FromSource("Microsoft.EntityFrameworkCore.Database.Command");

        var tablesToFilter = SearchValues.Create(
            tablesToFilterOverride is null ? OutboxInboxTablesToFilterOut.AsSpan() : tablesToFilterOverride.AsSpan(),
            StringComparison.Ordinal
        );
        var containingTablesPredicate = Matching.WithProperty<string>(
            "commandText",
            t => t.AsSpan().ContainsAny(tablesToFilter)
        );

        loggerConfiguration.Filter.ByExcluding(le =>
            le.Level < logLevel && fromSourcePredicate(le) && containingTablesPredicate(le)
        );

        return loggerConfiguration;
    }
}
