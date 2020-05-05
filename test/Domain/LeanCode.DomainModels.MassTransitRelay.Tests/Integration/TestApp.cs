using System;
using System.Data.Common;
using System.Threading.Tasks;
using Autofac;
using LeanCode.Components;
using LeanCode.Correlation;
using LeanCode.CQRS.Default;
using LeanCode.CQRS.Execution;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.DomainModels.Model;
using LeanCode.IdentityProvider;
using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.Integration
{
    public class TestApp : IAsyncLifetime, IDisposable
    {
        private static readonly TypesCatalog SearchAssemblies = new TypesCatalog(typeof(TestApp));
        private readonly AppModule[] modules = new AppModule[]
        {
            new CQRSModule().WithCustomPipelines<Context>(
                SearchAssemblies,
                cmd => cmd.Correlate().PublishEvents().InterceptEvents(),
                query => query),

            new MassTransitRelayModule("test-queue", SearchAssemblies),
            new CorrelationModule(),
        };

        private readonly IContainer container;
        private readonly IBusControl bus;
        public DbConnection DbConnection { get; set; }

        public Guid CorrelationId { get; }
        public ICommandExecutor<Context> Commands { get; }
        public HandledEvent[] HandledEvents<TEvent>()
            where TEvent : class, IDomainEvent
        {
            return container.Resolve<HandledEventsReporter<TEvent>>().HandledEvents;
        }

        public TestApp()
        {
            Serilog.Log.Logger = new Serilog.LoggerConfiguration().CreateLogger();

            DbConnection = new SqliteConnection("Filename=:memory:");

            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(HandledEventsReporter<>))
                .AsSelf()
                .SingleInstance();

            foreach (var m in modules)
            {
                builder.RegisterModule(m);
            }

            builder.Register(ctx => new TestDbContext(DbConnection))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            container = builder.Build();
            bus = container.Resolve<IBusControl>();
            Commands = container.Resolve<ICommandExecutor<Context>>();
            CorrelationId = Identity.NewId();
        }

        public void Dispose()
        {
            DbConnection.Dispose();
            container.Dispose();
        }

        public async Task InitializeAsync()
        {
            await bus.StartAsync();
            await DbConnection.OpenAsync();

            using var dbContext = new TestDbContext(DbConnection);
            await dbContext.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await bus.StopAsync();
            await DbConnection.CloseAsync();
        }
    }
}
