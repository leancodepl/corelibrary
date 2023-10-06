using Xunit;

namespace LeanCode.IntegrationTests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (Environment.GetEnvironmentVariable(TestDatabaseConfig.ConfigEnvName) != "postgres")
        {
            Skip = "Not running against PostgreSQL.";
        }
    }
}
