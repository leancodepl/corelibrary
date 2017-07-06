using System;
using System.Threading.Tasks;
using Autofac;
using LeanCode.DomainModels.Model;
using NSubstitute;
using Xunit;

namespace LeanCode.DomainModels.EventsExecutor.Tests
{
    public class EventsExecutorTests
    {
        private static readonly RetryPolicies policies = new RetryPolicies();
        private static readonly AsyncEventsStorage storage = new AsyncEventsStorage();

        public EventsExecutorTests()
        {
            ((IStartable)storage).Start();
        }

        [Fact]
        public async Task Calls_the_action_method()
        {
            bool called = false;
            var ee = Prepare();

            await ee.HandleEventsOf(() =>
            {
                called = true;
                return Task.FromResult(ExecutionResult.Process(ee));
            });

            Assert.True(called, "The action has not been called.");
        }

        [Fact]
        public async Task Sets_the_storage_before_execution()
        {
            var queue = await Prepare().HandleEventsOf(() =>
            {
                var q = storage.PeekQueue();
                return Task.FromResult(ExecutionResult.Process(q));
            });
            Assert.NotNull(queue);
        }

        [Fact]
        public async Task Resets_storage_after_execution()
        {
            await Prepare().HandleEventsOf(() =>
                Task.FromResult(ExecutionResult.Process(1)));

            Assert.Null(storage.PeekQueue());
        }

        [Fact]
        public async Task Executes_handler()
        {
            var h = Substitute.For<IDomainEventHandler<Event1>>();
            var e1 = new Event1();
            await Prepare(h).HandleEventsOf(Publish(e1));

            _ = h.Received(1).HandleAsync(e1);
        }

        [Fact]
        public async Task Executes_all_handler()
        {
            var h1 = Substitute.For<IDomainEventHandler<Event1>>();
            var h2 = Substitute.For<IDomainEventHandler<Event1>>();
            var e1 = new Event1();

            await Prepare(h1, h2).HandleEventsOf(Publish(e1));

            _ = h1.Received(1).HandleAsync(e1);
            _ = h2.Received(1).HandleAsync(e1);
        }

        [Fact]
        public async Task Executes_all_handler_for_all_events()
        {
            var h1 = Substitute.For<IDomainEventHandler<Event1>>();
            var h2 = Substitute.For<IDomainEventHandler<Event1>>();
            var h3 = Substitute.For<IDomainEventHandler<Event2>>();
            var h4 = Substitute.For<IDomainEventHandler<Event2>>();

            var e1 = new Event1();
            var e2 = new Event2();

            await Prepare(h1, h2, h3, h4).HandleEventsOf(Publish(e1, e2));

            _ = h1.Received(1).HandleAsync(e1);
            _ = h2.Received(1).HandleAsync(e1);
            _ = h3.Received(1).HandleAsync(e2);
            _ = h4.Received(1).HandleAsync(e2);
        }

        [Fact]
        public async Task Executes_handlers_for_events_raised_in_handler()
        {
            var e1 = new Event1();
            var e2 = new Event2();

            var h1 = new PublishingHandler<Event1>(e2);
            var h2 = Substitute.For<IDomainEventHandler<Event2>>();

            await Prepare(h1, h2).HandleEventsOf(Publish(e1));

            _ = h2.Received(1).HandleAsync(e2);
        }

        [Fact]
        public async Task Executes_all_handlers_for_events_raised_in_handler()
        {
            var e1 = new Event1();
            var e2 = new Event2();

            var h1 = new PublishingHandler<Event1>(e2);
            var h2 = Substitute.For<IDomainEventHandler<Event2>>();
            var h3 = Substitute.For<IDomainEventHandler<Event2>>();

            await Prepare(h1, h2, h3).HandleEventsOf(Publish(e1));

            _ = h2.Received(1).HandleAsync(e2);
            _ = h3.Received(1).HandleAsync(e2);
        }

        [Fact]
        public async Task Executes_all_events_even_if_they_are_the_same()
        {
            var e1 = new Event1();
            var e2 = new Event1();

            var h1 = Substitute.For<IDomainEventHandler<Event1>>();
            var h2 = Substitute.For<IDomainEventHandler<Event1>>();

            await Prepare(h1, h2).HandleEventsOf(Publish(e1, e2));

            _ = h1.Received(1).HandleAsync(e1);
            _ = h2.Received(1).HandleAsync(e1);
            _ = h1.Received(1).HandleAsync(e2);
            _ = h2.Received(1).HandleAsync(e2);
        }

        [Fact]
        public async Task Executes_all_events_raised_in_the_event_handler_even_if_they_are_of_the_same_type()
        {
            var e1 = new Event2();
            var e2 = new Event2();

            var pub = new PublishingHandler<Event1>(1, e1, e2);
            var h1 = Substitute.For<IDomainEventHandler<Event2>>();
            var h2 = Substitute.For<IDomainEventHandler<Event2>>();

            await Prepare(pub, h1, h2).HandleEventsOf(Publish(new Event1()));

            _ = h1.Received(1).HandleAsync(e1);
            _ = h2.Received(1).HandleAsync(e1);
            _ = h1.Received(1).HandleAsync(e2);
            _ = h2.Received(1).HandleAsync(e2);
        }

        private static IEventsExecutor Prepare(params object[] handlers)
        {
            var resolver = new HandlerResolver(handlers);
            return new EventsExecutor(policies, storage, resolver);
        }

        private static Func<Task<ExecutionResult<int>>> Publish(
            params IDomainEvent[] events)
        {
            return () =>
            {
                foreach (var e in events)
                {
                    DomainEvents.Raise(e);
                }
                return Task.FromResult(ExecutionResult.Process(1));
            };
        }

        private static Func<Task<ExecutionResult<int>>> Skip(
            params IDomainEvent[] events)
        {
            return () =>
            {
                foreach (var e in events)
                {
                    DomainEvents.Raise(e);
                }
                return Task.FromResult(ExecutionResult.Skip(1));
            };
        }
    }
}
