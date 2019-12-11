using System;
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

            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(HandledEventsReporter<>))
                .AsSelf()
                .SingleInstance();

            foreach (var m in modules)
            {
                builder.RegisterModule(m);
            }

            container = builder.Build();
            bus = container.Resolve<IBusControl>();
            Commands = container.Resolve<ICommandExecutor<Context>>();
            CorrelationId = Identity.NewId();
        }

        public void Dispose() => container.Dispose();

        public Task DisposeAsync() => bus.StopAsync();

        public Task InitializeAsync() => bus.StartAsync();
    }
}
