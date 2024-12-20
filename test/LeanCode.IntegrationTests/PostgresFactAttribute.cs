using LeanCode.Test.Helpers;

namespace LeanCode.IntegrationTests;

public sealed class PostgresFactAttribute : IntegrationFactAttribute
{
    public PostgresFactAttribute()
    {
        if (Environment.GetEnvironmentVariable(TestDatabaseConfig.ConfigEnvName) != "postgres")
        {
            Skip = "Not running against PostgreSQL.";
        }
    }
}
