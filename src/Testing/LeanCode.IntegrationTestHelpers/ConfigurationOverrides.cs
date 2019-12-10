using System;
using System.Collections.Generic;
using LeanCode.Components.Startup;
using Microsoft.Extensions.Configuration;
using Serilog.Events;

namespace LeanCode.IntegrationTestHelpers
{
    public class ConfigurationOverrides : IConfigurationSource
    {
        public const LogEventLevel MinimumLevelDefault = LogEventLevel.Warning;
        public const bool EnableInternalLogsDefault = false;
        public const string ConnectionStringBaseDefault = "ConnectionStrings__DatabaseBase";

        private readonly LogEventLevel minimumLevel = MinimumLevelDefault;
        private readonly bool enableInternalLogs = EnableInternalLogsDefault;
        private readonly string connectionStringBase = ConnectionStringBaseDefault;

        public ConfigurationOverrides() { }

        public ConfigurationOverrides(LogEventLevel minimumLevel)
        {
            this.minimumLevel = minimumLevel;
        }

        public ConfigurationOverrides(LogEventLevel minimumLevel, bool enableInternalLogs)
        {
            this.minimumLevel = minimumLevel;
            this.enableInternalLogs = enableInternalLogs;
        }

        public ConfigurationOverrides(LogEventLevel minimumLevel, bool enableInternalLogs, string connectionStringBase)
        {
            this.minimumLevel = minimumLevel;
            this.enableInternalLogs = enableInternalLogs;
            this.connectionStringBase = connectionStringBase;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new Provider(minimumLevel, enableInternalLogs, connectionStringBase);
        }

        private class Provider : ConfigurationProvider
        {
            private readonly LogEventLevel minimumLevel;
            private readonly bool enableInternalLogs;
            private readonly string connectionStringBase;

            public Provider(LogEventLevel minimumLevel, bool enableInternalLogs, string connectionStringBase)
            {
                this.minimumLevel = minimumLevel;
                this.enableInternalLogs = enableInternalLogs;
                this.connectionStringBase = connectionStringBase;
            }

            public override void Load()
            {
                var dbName = $"integration_tests_{Guid.NewGuid().ToString("N")}";
                var rest = Environment.GetEnvironmentVariable(connectionStringBase);
                var dbConnStr = $"Initial Catalog={dbName};" + rest;

                Data = new Dictionary<string, string>
                {
                    ["ConnectionStrings:Database"] = dbConnStr,
                    ["InternalBase"] = "http://localhost",
                    ["PublicBase"] = "http://localhost",
                    [IWebHostBuilderExtensions.EnableDetailedInternalLogsKey] = enableInternalLogs.ToString(),
                    [IWebHostBuilderExtensions.MinimumLogLevelKey] = minimumLevel.ToString(),
                };
            }
        }
    }
}
