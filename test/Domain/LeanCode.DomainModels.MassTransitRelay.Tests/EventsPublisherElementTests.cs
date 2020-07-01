using System;
using System.Threading;
using System.Threading.Tasks;
using LeanCode.Correlation;
using LeanCode.DomainModels.MassTransitRelay.Middleware;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;
using NSubstitute;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests
{
    [Collection("EventsInterceptor")]
    public class EventsPublisherElementTests
    {
        private readonly IEventPublisher publisher;
        private readonly EventsPublisherElement<TestContext, TestPayload, TestPayload> element;
        private readonly AsyncEventsInterceptor interceptor = new AsyncEventsInterceptor();

        private readonly TestContext testContext = new TestContext
        {
            CorrelationId = Guid.NewGuid(),
        };

        public EventsPublisherElementTests()
        {
            publisher = Substitute.For<IEventPublisher>();
            element = new EventsPublisherElement<TestContext, TestPayload, TestPayload>(publisher, interceptor);
            interceptor.Configure();
        }

        [Fact]
        public async Task Publishes_intercepted_events()
        {
            var evt1 = new TestEvent();
            var evt2 = new TestEvent();
            var evt3 = new TestEvent();

            Task<TestPayload> Next(TestContext context, TestPayload input)
            {
                DomainEvents.Raise(evt1);
                DomainEvents.Raise(evt2);
                DomainEvents.Raise(evt3);
                return Task.FromResult(input);
            }

            await element.ExecuteAsync(testContext, new TestPayload(), Next);

            await publisher.Received(1).PublishAsync(evt1, testContext.CorrelationId);
            await publisher.Received(1).PublishAsync(evt2, testContext.CorrelationId);
            await publisher.Received(1).PublishAsync(evt3, testContext.CorrelationId);
        }

        [Fact]
        public async Task If_publishing_one_event_fails_the_rest_are_not_interrupted()
        {
            var evt1 = new TestEvent();
            var evt2 = new TestEvent();
            var evt3 = new TestEvent();
            var inp = new TestPayload();

            Task<TestPayload> Next(TestContext context, TestPayload input)
            {
                DomainEvents.Raise(evt1);
                DomainEvents.Raise(evt2);
                DomainEvents.Raise(evt3);

                return Task.FromResult(input);
            }

            publisher.PublishAsync(evt2, Arg.Any<Guid>()).Returns(x => throw new Exception());

            await element.ExecuteAsync(testContext, inp, Next);

            await publisher.Received(1).PublishAsync(evt1, testContext.CorrelationId);
            await publisher.Received(1).PublishAsync(evt3, testContext.CorrelationId);
        }

        [Fact]
        public async Task Exceptions_thrown_by_next_elements_of_the_pipeline_are_propagated()
        {
            var ctx = new TestContext();
            var input = new TestPayload();
            var next = Substitute.For<Func<TestContext, TestPayload, Task<TestPayload>>>();
            next(null, null).ReturnsForAnyArgs<TestPayload>(x => throw new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => element.ExecuteAsync(ctx, input, next));
        }
    }

    public class TestEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime DateOccurred { get; }
    }

    public class TestPayload
    { }

    public class TestContext : ICorrelationContext
    {
        public Guid CorrelationId { get; set; }
        public Guid ExecutionId { get; set; }
        public IPipelineScope Scope { get; set; }
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
    }
}
