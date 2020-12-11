using System;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using GreenPipes;
using LeanCode.Components;
using LeanCode.Correlation;
using LeanCode.CQRS.Default;
using LeanCode.CQRS.Execution;
using LeanCode.DomainModels.MassTransitRelay.Middleware;
using LeanCode.DomainModels.MassTransitRelay.Testing;
using LeanCode.DomainModels.Model;
using LeanCode.IdentityProvider;
using MassTransit;
using MassTransit.AutofacIntegration;
using MassTransit.Testing.Indicators;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.Integration
{
    public class TestApp : IAsyncLifetime, IDisposable
    {
        private static readonly TypesCatalog SearchAssemblies = TypesCatalog.Of<TestApp>();
        private readonly AppModule[] modules = new AppModule[]
        {
            new CQRSModule().WithCustomPipelines<Context>(
                SearchAssemblies,
                cmd => cmd.Correlate().StoreAndPublishEvents(),
                query => query),

            new MassTransitRelayModule(SearchAssemblies, SearchAssemblies, MassTransitTestRelayModule.TestBusConfigurator),
            new MassTransitTestRelayModule(),
            new CorrelationModule(),
        };

        private readonly IBusControl bus;
        private readonly SqliteConnection dbConnection;

        public Guid CorrelationId { get; }
        public IContainer Container { get; }
        public ICommandExecutor<Context> Commands { get; }
        public IBusActivityMonitor ActivityMonitor { get; }

        public HandledEvent[] HandledEvents<TEvent>()
            where TEvent : class, IDomainEvent
        {
            return Container.Resolve<HandledEventsReporter<TEvent>>().HandledEvents;
        }

        public TestApp()
        {
            Log.Logger = new LoggerConfiguration().CreateLogger();

            // We have to use the same in-mem database but differnet connections so that we don't have
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
            dbConnection = new SqliteConnection(connStr.ConnectionString);

            var services = new ServiceCollection();
            services.AddLogging(cfg => cfg.AddSerilog());
            services.AddDbContext<TestDbContext>(cfg => cfg.UseSqlite(connStr.ConnectionString));

            var builder = new ContainerBuilder();
            builder.Populate(services);

            builder.Register(c => c.Resolve<TestDbContext>())
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterGeneric(typeof(HandledEventsReporter<>))
                .AsSelf()
                .SingleInstance();

            foreach (var m in modules)
            {
                builder.RegisterModule(m);
            }

            Container = builder.Build();
            bus = Container.Resolve<IBusControl>();

            CorrelationId = Identity.NewId();
            Commands = Container.Resolve<ICommandExecutor<Context>>();
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

            await bus.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await bus.StopAsync();
            await dbConnection.CloseAsync();
            await dbConnection.DisposeAsync();
        }
    }
}
