#nullable enable
using Autofac;
using Autofac.Extensions.DependencyInjection;
using LeanCode.CQRS.MassTransitRelay.Middleware;
using LeanCode.DomainModels.Model;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.CQRS.MassTransitRelay.Tests.Middleware;

[Collection("EventsInterceptor")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1849", Justification = "Allowed in tests.")]
public sealed class EventsPublisherFilterTests : IAsyncLifetime, IDisposable
{
    private readonly IContainer container;
    private readonly InMemoryTestHarness harness;

    public EventsPublisherFilterTests()
    {
        var collection = new ServiceCollection();
        collection.AddMassTransitInMemoryTestHarness();

        var factory = new AutofacServiceProviderFactory();
        var builder = factory.CreateBuilder(collection);
        builder
            .RegisterType<AsyncEventsInterceptor>()
            .AsSelf()
            .OnActivated(a => a.Instance.Configure())
            .SingleInstance();

        container = builder.Build();
        harness = container.Resolve<InMemoryTestHarness>();

        harness.TestTimeout = TimeSpan.FromSeconds(1);
        harness.OnConfigureInMemoryBus += cfg =>
        {
            cfg.UseDomainEventsPublishing(container.Resolve<IServiceProvider>());
        };
    }

    [Fact]
    public async Task Publishes_event_to_the_bus()
    {
        var consumerHarness = harness.Consumer<Consumer>();
        await harness.Start();

        await harness.Bus.Publish(new TestMsg());
        Assert.True(await consumerHarness.Consumed.Any<TestMsg>());

        var evt = Assert.Single(harness.Published.Select<TestEvent>());
        Assert.Equal(Consumer.Event, evt.MessageObject);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await harness.Stop();
        await container.DisposeAsync();
    }

    public void Dispose()
    {
        harness.Dispose();
        container.Dispose();
    }

    private sealed class TestMsg { }

    private sealed class TestEvent : IDomainEvent
    {
        public Guid Id { get; set; }

        public DateTime DateOccurred { get; set; }
    }

    private sealed class Consumer : IConsumer<TestMsg>
    {
        public static readonly TestEvent Event = new() { Id = Guid.NewGuid() };

        public Task Consume(ConsumeContext<TestMsg> context)
        {
            DomainEvents.Raise(Event);
            return Task.CompletedTask;
        }
    }
}
