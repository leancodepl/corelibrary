using LeanCode.CQRS.MassTransitRelay.LockProviders;
using LeanCode.IntegrationTestHelpers;
using LeanCode.IntegrationTests.App;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LeanCode.IntegrationTests;

public abstract class TestDatabaseConfig
{
    public abstract ConfigurationOverrides GetConfigurationOverrides();
    public abstract void ConfigureDbContext(DbContextOptionsBuilder builder, IConfiguration config);
    public abstract void ConfigureMassTransitOutbox(IEntityFrameworkOutboxConfigurator configurator);

    public static TestDatabaseConfig Create()
    {
        return new MssqlTestDatabaseConfig();
    }
}

public class MssqlTestDatabaseConfig : TestDatabaseConfig
{
    public override ConfigurationOverrides GetConfigurationOverrides() => new(
        "SqlServer__ConnectionStringBase",
        "SqlServer:ConnectionString");

    public override void ConfigureDbContext(DbContextOptionsBuilder builder, IConfiguration config)
    {
        builder.UseSqlServer(config.GetValue<string>("SqlServer:ConnectionString"));
    }

    public override void ConfigureMassTransitOutbox(IEntityFrameworkOutboxConfigurator configurator)
    {
        configurator.LockStatementProvider = new CustomSqlServerLockStatementProvider();
    }
}

