#nullable enable
using LeanCode.CQRS.MassTransitRelay.Middleware;
using LeanCode.DomainModels.Model;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.CQRS.MassTransitRelay.Tests.Middleware;

[Collection("EventsInterceptor")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1849", Justification = "Allowed in tests.")]
public sealed class EventsPublisherFilterTests : IAsyncLifetime
{
    private readonly IServiceProvider serviceProvider;
    private readonly ITestHarness harness;

    public EventsPublisherFilterTests()
    {
        var collection = new ServiceCollection();
        collection.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumer<TestConsumer>();
            cfg.UsingInMemory(
                (ctx, bus) =>
                {
                    bus.UseDomainEventsPublishing(serviceProvider!);
                    bus.ConfigureEndpoints(ctx);
                }
            );
        });
        collection.AddAsyncEventsInterceptor();

        serviceProvider = collection.BuildServiceProvider();
        harness = serviceProvider.GetRequiredService<ITestHarness>();
        harness.TestTimeout = TimeSpan.FromSeconds(1);
    }

    [Fact]
    public async Task Publishes_event_to_the_bus()
    {
        var consumerHarness = harness.GetConsumerHarness<TestConsumer>();
        await harness.Start();

        await harness.Bus.Publish(new TestMsg());
        Assert.True(await consumerHarness.Consumed.Any<TestMsg>());

        var evt = Assert.Single(harness.Published.Select<TestEvent>());
        Assert.Equal(TestConsumer.Event, evt.MessageObject);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await harness.Stop();
    }

    private sealed class TestMsg { }

    private sealed class TestEvent : IDomainEvent
    {
        public Guid Id { get; set; }

        public DateTime DateOccurred { get; set; }
    }

    private sealed class TestConsumer : IConsumer<TestMsg>
    {
        public static readonly TestEvent Event = new() { Id = Guid.NewGuid() };

        public Task Consume(ConsumeContext<TestMsg> context)
        {
            DomainEvents.Raise(Event);
            return Task.CompletedTask;
        }
    }
}
