using Microsoft.Extensions.Configuration;

namespace LeanCode.IntegrationTestHelpers;

public class ConfigurationOverrides : IConfigurationSource
{
    public const string ConnectionStringBaseDefault = "SqlServer__ConnectionStringBase";
    public const string ConnectionStringKeyDefault = "SqlServer:ConnectionString";

    private readonly Dictionary<string, string?> data = new();

    public ConfigurationOverrides() { }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new Provider(data);
    }

    public ConfigurationOverrides Add(string key, string? value)
    {
        data[key] = value;
        return this;
    }

    public ConfigurationOverrides AddConnectionString(
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
}
