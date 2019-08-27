using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GreenPipes.Pipes;
using LeanCode.Correlation;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;
using MassTransit;
using NSubstitute;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests
{
    public class EventsPublisherElementTests
    {
        private readonly IBus bus;
        private readonly EventsPublisherElement<TestContext, TestPayload, TestPayload> publisher;

        private LastPipe<PublishContext<IDomainEvent>> PublishCallback => Arg.Any<LastPipe<PublishContext<IDomainEvent>>>();

        public EventsPublisherElementTests()
        {
            bus = Substitute.For<IBus>();
            publisher = new EventsPublisherElement<TestContext, TestPayload, TestPayload>(bus);
        }

        [Fact]
        public async Task Publishes_intercepted_events()
        {
            var evt1 = new TestEvent();
            var evt2 = new TestEvent();
            var evt3 = new TestEvent();
            var ctx = new TestContext();

            Task<TestPayload> Next(TestContext context, TestPayload input)
            {
                context.SavedEvents = new List<IDomainEvent> { evt1, evt2, evt3 };
                return Task.FromResult(input);
            }

            await publisher.ExecuteAsync(ctx, new TestPayload(), Next);

            await bus.Received(1).Publish(evt1, PublishCallback);
            await bus.Received(1).Publish(evt2, PublishCallback);
            await bus.Received(1).Publish(evt3, PublishCallback);
        }

        [Fact]
        public async Task If_publishing_one_event_fails_the_rest_are_not_interrupted()
        {
            var evt1 = new TestEvent();
            var evt2 = new TestEvent();
            var evt3 = new TestEvent();
            var ctx = new TestContext();
            var inp = new TestPayload();

            Task<TestPayload> Next(TestContext context, TestPayload input)
            {
                context.SavedEvents = new List<IDomainEvent> { evt1, evt2, evt3 };
                return Task.FromResult(input);
            }

            bus.Publish(evt2, PublishCallback).Returns(x => throw new Exception());

            await publisher.ExecuteAsync(ctx, inp, Next);

            await bus.Received(1).Publish(evt1, PublishCallback);
            await bus.Received(1).Publish(evt3, PublishCallback);
        }

        [Fact]
        public async Task Exceptions_thrown_by_next_elements_of_the_pipeline_are_propagated()
        {
            var ctx = new TestContext();
            var input = new TestPayload();
            var next = Substitute.For<Func<TestContext, TestPayload, Task<TestPayload>>>();
            next(null, null).ReturnsForAnyArgs<TestPayload>(x => throw new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => publisher.ExecuteAsync(ctx, input, next));
        }
    }

    public class TestEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime DateOccurred { get; }
    }

    public class TestPayload
    { }

    public class TestContext : IEventsInterceptorContext, ICorrelationContext
    {
        public Guid CorrelationId { get; set; }
        public Guid ExecutionId { get; set; }
        public IPipelineScope Scope { get; set; }
        public List<IDomainEvent> SavedEvents { get; set; }
    }
}
