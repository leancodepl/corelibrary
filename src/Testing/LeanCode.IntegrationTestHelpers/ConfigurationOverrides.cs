using Microsoft.Extensions.Configuration;
using Serilog.Events;

namespace LeanCode.IntegrationTestHelpers;

public class ConfigurationOverrides : IConfigurationSource
{
    public const LogEventLevel MinimumLevelDefault = LogEventLevel.Warning;
    public const bool EnableInternalLogsDefault = false;

    private readonly LogEventLevel minimumLevel;
    private readonly bool enableInternalLogs;
    private readonly string connectionStringBase;
    private readonly string connectionStringKey;

    public ConfigurationOverrides(
        string connectionStringBase,
        string connectionStringKey,
        LogEventLevel? minimumLevel = null,
        bool? enableInternalLogs = null
    )
    {
        this.connectionStringBase = connectionStringBase;
        this.connectionStringKey = connectionStringKey;
        this.minimumLevel = minimumLevel ?? MinimumLevelDefault;
        this.enableInternalLogs = enableInternalLogs ?? EnableInternalLogsDefault;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new Provider(minimumLevel, enableInternalLogs, connectionStringBase, connectionStringKey);
    }

    private sealed class Provider : ConfigurationProvider
    {
        private readonly LogEventLevel minimumLevel;
        private readonly bool enableInternalLogs;
        private readonly string connectionStringBase;
        private readonly string connectionStringKey;

        public Provider(
            LogEventLevel minimumLevel,
            bool enableInternalLogs,
            string connectionStringBase,
            string connectionStringKey
        )
        {
            this.minimumLevel = minimumLevel;
            this.enableInternalLogs = enableInternalLogs;
            this.connectionStringBase = connectionStringBase;
            this.connectionStringKey = connectionStringKey;
        }

        public override void Load()
        {
            var dbName = $"integration_tests_{Guid.NewGuid():N}";
            var rest = Environment.GetEnvironmentVariable(connectionStringBase);
            var dbConnStr = $"Database={dbName};" + rest;

            Data = new Dictionary<string, string?>
            {
                [connectionStringKey] = dbConnStr,
                [LeanCode.Logging.IHostBuilderExtensions.EnableDetailedInternalLogsKey] = enableInternalLogs.ToString(),
                [LeanCode.Logging.IHostBuilderExtensions.MinimumLogLevelKey] = minimumLevel.ToString(),
            };
        }
    }
}
