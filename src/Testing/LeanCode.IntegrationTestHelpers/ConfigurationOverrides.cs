using LeanCode.Components.Startup;
using Microsoft.Extensions.Configuration;
using Serilog.Events;

namespace LeanCode.IntegrationTestHelpers;

public class ConfigurationOverrides : IConfigurationSource
{
    public const string ConnectionStringBaseDefault = "SqlServer__ConnectionStringBase";
    public const string ConnectionStringKeyDefault = "SqlServer:ConnectionString";

    private readonly Dictionary<string, string?> data;

    private ConfigurationOverrides(Dictionary<string, string?> data)
    {
        this.data = data;
    }

    public static OverridesBuilder CreateBuilder() => new();

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new Provider(data);
    }

    private sealed class Provider : ConfigurationProvider
    {
        private readonly Dictionary<string, string?> data;

        public Provider(Dictionary<string, string?> data)
        {
            this.data = data;
        }

        public override void Load()
        {
            Data = data;
        }
    }

    public static string OverrideDatabaseConnectionString(
        string dbPrefix,
        string sourceEnv = ConnectionStringBaseDefault,
        string varName = "Database"
    )
    {
        var dbName = $"{dbPrefix}_{Guid.NewGuid():N}";
        var rest = Environment.GetEnvironmentVariable(sourceEnv);
        return $"{varName}={dbName};" + rest;
    }

    public class OverridesBuilder
    {
        private readonly Dictionary<string, string?> data = new();

        public OverridesBuilder AddValue(string key, string? value)
        {
            data[key] = value;
            return this;
        }

        public OverridesBuilder AddConnectionString(
            string key,
            string dbPrefix,
            string sourceEnv = ConnectionStringBaseDefault,
            string varName = "Database"
        )
        {
            var connectionString = OverrideDatabaseConnectionString(dbPrefix, sourceEnv, varName);
            data[key] = connectionString;
            return this;
        }

        public ConfigurationOverrides Build() => new(data);
    }
}
