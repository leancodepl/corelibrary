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
        public const string ConnectionStringKeyDefault = "ConnectionStrings:Database";

        private readonly LogEventLevel minimumLevel;
        private readonly bool enableInternalLogs;
        private readonly string connectionStringBase;
        private readonly string connectionStringKey;

        public ConfigurationOverrides(
            LogEventLevel minimumLevel = MinimumLevelDefault,
            bool enableInternalLogs = EnableInternalLogsDefault,
            string connectionStringBase = ConnectionStringBaseDefault,
            string connectionStringKey = ConnectionStringKeyDefault)
        {
            this.minimumLevel = minimumLevel;
            this.enableInternalLogs = enableInternalLogs;
            this.connectionStringBase = connectionStringBase;
            this.connectionStringKey = connectionStringKey;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new Provider(minimumLevel, enableInternalLogs, connectionStringBase, connectionStringKey);
        }

        private class Provider : ConfigurationProvider
        {
            private readonly LogEventLevel minimumLevel;
            private readonly bool enableInternalLogs;
            private readonly string connectionStringBase;
            private readonly string connectionStringKey;

            public Provider(
                LogEventLevel minimumLevel,
                bool enableInternalLogs,
                string connectionStringBase,
                string connectionStringKey)
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
                var dbConnStr = $"Initial Catalog={dbName};" + rest;

                Data = new Dictionary<string, string>
                {
                    [connectionStringKey] = dbConnStr,
                    ["InternalBase"] = "http://localhost",
                    ["PublicBase"] = "http://localhost",
                    [IHostBuilderExtensions.EnableDetailedInternalLogsKey] = enableInternalLogs.ToString(),
                    [IHostBuilderExtensions.MinimumLogLevelKey] = minimumLevel.ToString(),
                };
            }
        }
    }
}
