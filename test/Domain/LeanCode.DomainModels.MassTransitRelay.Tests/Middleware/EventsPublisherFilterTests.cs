#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using GreenPipes.Internals.Extensions;
using LeanCode.DomainModels.MassTransitRelay.Middleware;
using LeanCode.DomainModels.Model;
using MassTransit;
using MassTransit.AutofacIntegration;
using MassTransit.Registration;
using MassTransit.Testing;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.Middleware
{
    [Collection("EventsInterceptor")]
    public class EventsPublisherFilterTests : IAsyncLifetime, IDisposable
    {
        private readonly IContainer container;
        private readonly InMemoryTestHarness harness;

        public EventsPublisherFilterTests()
        {
            var builder = new ContainerBuilder();
            builder.AddMassTransitInMemoryTestHarness();
            builder.RegisterType<AsyncEventsInterceptor>().AsSelf().OnActivated(a => a.Instance.Configure()).SingleInstance();

            container = builder.Build();
            harness = container.Resolve<InMemoryTestHarness>();

            harness.TestTimeout = TimeSpan.FromSeconds(1);
            harness.OnConfigureInMemoryBus += cfg =>
            {
                cfg.UseDomainEventsPublishing(container.Resolve<IConfigurationServiceProvider>());
            };
        }

        [Fact]
        public async Task Publishes_event_to_the_bus()
        {
            var consumerHarness = harness.Consumer<Consumer>();
            await harness.Start();

            await harness.Bus.Publish(new TestMsg());
            Assert.True(await consumerHarness.Consumed.Any<TestMsg>());

            var evt = await AssertSingleAsync(harness.Published.SelectAsync<TestEvent>());
            Assert.Equal(Consumer.Event, evt.MessageObject);
        }

        private static async Task<T> AssertSingleAsync<T>(IAsyncEnumerable<T> e)
        {
            return await SingleAsync(e).OrTimeout(s: 1);

            static async Task<T> SingleAsync(IAsyncEnumerable<T> e)
            {
                var enumerator = e.GetAsyncEnumerator();
                Assert.True(await enumerator.MoveNextAsync(), "Enumerable should return one element but returned none.");
                var value = enumerator.Current;
                Assert.False(await enumerator.MoveNextAsync(), "Enumerable should return one element but returned more than one.");
                return value;
            }
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

        private class TestMsg { }

        private class TestEvent : IDomainEvent
        {
            public Guid Id { get; set; }

            public DateTime DateOccurred { get; set; }
        }

        private class Consumer : IConsumer<TestMsg>
        {
            public static readonly TestEvent Event = new() { Id = Guid.NewGuid() };

            public Task Consume(ConsumeContext<TestMsg> context)
            {
                DomainEvents.Raise(Event);
                return Task.CompletedTask;
            }
        }
    }
}
