using Autofac;
using Autofac.Extensions.DependencyInjection;
using LeanCode.Components;
using LeanCode.DomainModels.MassTransitRelay.Middleware;
using LeanCode.DomainModels.MassTransitRelay.Testing;
using LeanCode.OpenTelemetry;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.Testing;
using MassTransit.Testing.Implementations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.Integration;

public sealed class TestApp : IAsyncLifetime, IDisposable
{
    private readonly AppModule[] modules = new AppModule[]
    {
        // TODO: restore
        // new CQRSModule().WithCustomPipelines<Context>(
        //     SearchAssemblies,
        //     cmd => cmd.Trace().StoreAndPublishEvents(),
        //     query => query,
        //     op => op
        // ),
        new TestMassTransitModule(),
        new MassTransitTestRelayModule(),
        new OpenTelemetryModule(),
    };

    private readonly IBusControl bus;
    private readonly SqliteConnection dbConnection;

    public Guid CorrelationId { get; }
    public IContainer Container { get; }
    // public ICommandExecutor<Context> Commands { get; }
    public IBusActivityMonitor ActivityMonitor { get; }
    public ITestHarness Harness => Container.Resolve<ITestHarness>();

    public TestApp()
    {
        Log.Logger = new LoggerConfiguration().CreateLogger();

        // We have to use the same in-mem database but different connections so that we don't have
        // overlapping transactions but still be able to use the same data.
        var connStr = new SqliteConnectionStringBuilder
        {
            DataSource = Guid.NewGuid().ToString("N"),
            Mode = SqliteOpenMode.Memory,
            Cache = SqliteCacheMode.Shared,
        };
        // The database is destroyed when the last connection to it closes, hence we need
        // to have one artificial connection to it for the duration of the test, otherwise
        // it will get dropped prematurely and the tests would fail.
        dbConnection = new(connStr.ConnectionString);

        var services = new ServiceCollection();
        services.AddLogging(cfg => cfg.AddSerilog());
        services.AddDbContext<TestDbContext>(cfg => cfg.UseSqlite(connStr.ConnectionString));

        var builder = new ContainerBuilder();

        foreach (var m in modules)
        {
            m.ConfigureServices(services);
            builder.RegisterModule(m);
        }

        builder.Populate(services);
        Container = builder.Build();
        bus = Container.Resolve<IBusControl>();

        CorrelationId = Guid.NewGuid();
        // Commands = Container.Resolve<ICommandExecutor<Context>>();
        ActivityMonitor = Container.Resolve<IBusActivityMonitor>();
    }

    public void Dispose()
    {
        Container.Dispose();
    }

    public async Task InitializeAsync()
    {
        await dbConnection.OpenAsync();

        await using var scope = Container.BeginLifetimeScope();
        using var dbContext = scope.Resolve<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        await Harness.Start();
    }

    public async Task DisposeAsync()
    {
        await Harness.Stop();
        await dbConnection.CloseAsync();
        await dbConnection.DisposeAsync();
    }
}

public class TestMassTransitModule : MassTransitRelayModule
{
    public override void ConfigureMassTransit(IServiceCollection services)
    {
        services
            .AddOptions<OutboxDeliveryServiceOptions>()
            .Configure(opts => opts.QueryDelay = TimeSpan.FromSeconds(1));

        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumersWithDefaultConfiguration(
                new[] { typeof(TestApp).Assembly },
                typeof(DefaultConsumerDefinition<>)
            );

            cfg.AddEntityFrameworkOutbox<TestDbContext>(outboxCfg =>
            {
                outboxCfg.UseSqlite();
                outboxCfg.UseBusOutbox();
            });

            cfg.UsingInMemory(
                (ctx, busCfg) =>
                {
                    busCfg.ConfigureEndpoints(ctx);
                    busCfg.ConnectBusObservers(ctx);
                }
            );
        });
    }
}
