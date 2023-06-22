using Microsoft.Extensions.Configuration;
using Serilog.Events;

namespace LeanCode.IntegrationTestHelpers;

public class ConfigurationOverrides : IConfigurationSource
{
    public const LogEventLevel MinimumLevelDefault = LogEventLevel.Warning;
    public const bool EnableInternalLogsDefault = false;
    public const string InternalBaseKeyDefault = "InternalBase";
    public const string PublicBaseKeyDefault = "PublicBase";

    private readonly LogEventLevel minimumLevel;
    private readonly bool enableInternalLogs;
    private readonly string connectionStringBase;
    private readonly string connectionStringKey;
    private readonly string internalBaseKey;
    private readonly string publicBaseKey;

    public ConfigurationOverrides(
        string connectionStringBase,
        string connectionStringKey,
        LogEventLevel? minimumLevel = null,
        bool? enableInternalLogs = null,
        string? internalBaseKey = null,
        string? publicBaseKey = null
    )
    {
        this.connectionStringBase = connectionStringBase;
        this.connectionStringKey = connectionStringKey;
        this.minimumLevel = minimumLevel ?? MinimumLevelDefault;
        this.enableInternalLogs = enableInternalLogs ?? EnableInternalLogsDefault;
        this.internalBaseKey = internalBaseKey ?? InternalBaseKeyDefault;
        this.publicBaseKey = publicBaseKey ?? PublicBaseKeyDefault;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new Provider(
            minimumLevel,
            enableInternalLogs,
            connectionStringBase,
            connectionStringKey,
            internalBaseKey,
            publicBaseKey
        );
    }

    private sealed class Provider : ConfigurationProvider
    {
        private readonly LogEventLevel minimumLevel;
        private readonly bool enableInternalLogs;
        private readonly string connectionStringBase;
        private readonly string connectionStringKey;
        private readonly string internalBaseKey;
        private readonly string publicBaseKey;

        public Provider(
            LogEventLevel minimumLevel,
            bool enableInternalLogs,
            string connectionStringBase,
            string connectionStringKey,
            string internalBaseKey,
            string publicBaseKey
        )
        {
            this.minimumLevel = minimumLevel;
            this.enableInternalLogs = enableInternalLogs;
            this.connectionStringBase = connectionStringBase;
            this.connectionStringKey = connectionStringKey;
            this.internalBaseKey = internalBaseKey;
            this.publicBaseKey = publicBaseKey;
        }

        public override void Load()
        {
            var dbName = $"integration_tests_{Guid.NewGuid():N}";
            var rest = Environment.GetEnvironmentVariable(connectionStringBase);
            var dbConnStr = $"Database={dbName};" + rest;

            Data = new Dictionary<string, string?>
            {
                [connectionStringKey] = dbConnStr,
                [internalBaseKey] = "http://localhost",
                [publicBaseKey] = "http://localhost",
                [LeanCode.Logging.IHostBuilderExtensions.EnableDetailedInternalLogsKey] = enableInternalLogs.ToString(),
                [LeanCode.Logging.IHostBuilderExtensions.MinimumLogLevelKey] = minimumLevel.ToString(),
            };
        }
    }
}
