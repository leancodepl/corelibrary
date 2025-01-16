using LeanCode.DomainModels.EF;
using LeanCode.IntegrationTestHelpers;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace LeanCode.IntegrationTests;

public abstract class TestDatabaseConfig
{
    public const string ConfigEnvName = "LeanCodeIntegrationTests__Database";

    public abstract TestConnectionString GetConnectionString();
    public abstract void ConfigureDbContext(DbContextOptionsBuilder builder, IConfiguration config);
    public abstract void ConfigureMassTransitOutbox(IEntityFrameworkOutboxConfigurator configurator);

    public static TestDatabaseConfig Create()
    {
        return Environment.GetEnvironmentVariable(ConfigEnvName) switch
        {
            "sqlserver" => new SqlServerTestDatabaseConfig(),
            "postgres" => new PostgresTestConfig(),
            _ => throw new InvalidOperationException(
                $"Set the database provider (sqlserver|postgres) via {ConfigEnvName} env variable"
            ),
        };
    }
}

public class SqlServerTestDatabaseConfig : TestDatabaseConfig
{
    public override TestConnectionString GetConnectionString() =>
        new("SqlServer:ConnectionStringBase", "SqlServer:ConnectionString");

    public override void ConfigureDbContext(DbContextOptionsBuilder builder, IConfiguration config)
    {
        builder.UseSqlServer(config.GetValue<string>("SqlServer:ConnectionString"));
    }

    public override void ConfigureMassTransitOutbox(IEntityFrameworkOutboxConfigurator configurator)
    {
        configurator.LockStatementProvider = new SqlServerLockStatementProvider();
    }
}

public class PostgresTestConfig : TestDatabaseConfig
{
    public override TestConnectionString GetConnectionString() =>
        new("Postgres:ConnectionStringBase", "Postgres:ConnectionString");

    public override void ConfigureDbContext(DbContextOptionsBuilder builder, IConfiguration config)
    {
        builder.UseNpgsql(config.GetValue<string>("Postgres:ConnectionString")).AddTimestampTzExpressionInterceptor();
    }

    public override void ConfigureMassTransitOutbox(IEntityFrameworkOutboxConfigurator configurator)
    {
        configurator.LockStatementProvider = new PostgresLockStatementProvider();
    }
}
