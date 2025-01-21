using Microsoft.Extensions.Configuration;

namespace LeanCode.IntegrationTestHelpers;

public class TestConnectionString(string connectionStringBaseKey, string connectionStringKey) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var newBuilder = RebuildWithoutSelf(builder);
        return new Provider(connectionStringBaseKey, connectionStringKey, newBuilder);
    }

    private IConfigurationRoot RebuildWithoutSelf(IConfigurationBuilder builder)
    {
        var newBuilder = new ConfigurationBuilder();
        foreach (var src in builder.Sources)
        {
            if (src != this)
            {
                newBuilder.Add(src);
            }
        }

        return newBuilder.Build();
    }

    private sealed class Provider(
        string connectionStringBaseKey,
        string connectionStringKey,
        IConfiguration parentConfig
    ) : ConfigurationProvider
    {
        public override void Load()
        {
            var baseConnStr =
                parentConfig[connectionStringBaseKey]
                ?? throw new KeyNotFoundException(
                    $"Cannot find base connection string under key {connectionStringBaseKey}"
                );

            var dbName = $"integration_tests_{Guid.NewGuid():N}";
            var dbConnStr = $"Database={dbName};" + baseConnStr;

            Data = new Dictionary<string, string?> { [connectionStringKey] = dbConnStr };
        }
    }
}
