using System.Net.Http.Json;
using System.Text.Json;
using LeanCode.Components;
using LeanCode.Contracts;
using LeanCode.CQRS.AspNetCore;
using MassTransit;
using MassTransit.Testing;
using MassTransit.Testing.Implementations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Xunit;

namespace LeanCode.CQRS.MassTransitRelay.Tests.Integration;

public sealed class TestApp : IAsyncLifetime, IDisposable
{
    private readonly SqliteConnection dbConnection;
    private readonly IHost host;
    private readonly TestServer server;

    public IBusActivityMonitor ActivityMonitor => host.Services.GetRequiredService<IBusActivityMonitor>();
    public ITestHarness Harness => host.Services.GetRequiredService<ITestHarness>();
    public IServiceProvider Services => host.Services;

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

        host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost
                    .UseTestServer()
                    .ConfigureServices(cfg =>
                    {
                        cfg.AddDbContext<TestDbContext>(db => db.UseSqlite(connStr.ConnectionString));
                        cfg.AddLogging(l => l.AddSerilog());
                        cfg.AddRouting();
                        cfg.AddCQRS(TypesCatalog.Of<TestCommand>(), TypesCatalog.Of<TestCommandHandler>());
                        cfg.AddMassTransitTestHarness(ConfigureMassTransit);
                        cfg.AddAsyncEventsInterceptor();
                        cfg.AddBusActivityMonitor();
                        cfg.AddOptions<OutboxDeliveryServiceOptions>()
                            .Configure(opts => opts.QueryDelay = TimeSpan.FromSeconds(1));
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(ep =>
                        {
                            ep.MapRemoteCqrs(
                                "/cqrs",
                                cqrs =>
                                {
                                    cqrs.Queries = q => q.Secure();
                                    cqrs.Commands = c =>
                                        c.Secure().Validate().CommitTransaction<TestDbContext>().PublishEvents();
                                    cqrs.Operations = o => o.Secure();
                                }
                            );
                        });
                    });
            })
            .Build();

        server = host.GetTestServer();
        Harness.TestInactivityTimeout = TimeSpan.FromSeconds(0.5);
    }

    private static void ConfigureMassTransit(IBusRegistrationConfigurator cfg)
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
    }

    public async Task RunCommand(ICommand command)
    {
        var response = await server
            .CreateRequest($"/cqrs/command/{command.GetType().FullName}")
            .And(
                cfg =>
                    cfg.Content = JsonContent.Create(command, command.GetType(), options: new JsonSerializerOptions())
            )
            .PostAsync();
        response.EnsureSuccessStatusCode();
    }

    public void Dispose()
    {
        server.Dispose();
        host.Dispose();
    }

    public async Task InitializeAsync()
    {
        await dbConnection.OpenAsync();

        using var scope = host.Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        await host.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await host.StopAsync();
        await dbConnection.CloseAsync();
        await dbConnection.DisposeAsync();
    }
}
