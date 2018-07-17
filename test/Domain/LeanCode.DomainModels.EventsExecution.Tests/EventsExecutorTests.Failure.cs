using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;
using LeanCode.Test.Helpers;
using NSubstitute;
using Xunit;

namespace LeanCode.DomainModels.EventsExecution.Tests
{
    public class EventsExecutorTests__Failure
    {
        private static readonly RetryPolicies policies = new RetryPolicies();
        private static readonly AsyncEventsInterceptor interceptor = new AsyncEventsInterceptor();

        public EventsExecutorTests__Failure()
        {
            interceptor.Configure();
        }

        [LongRunningFact]
        public async Task If_handler_fails_the_call_succeedes()
        {
            var h = new FailingHandler<Event1>();

            await Prepare(h).HandleEventsOf(Publish(new Event1()));
        }

        [LongRunningFact]
        public async Task If_handler_fails_Other_handlers_are_still_executed()
        {
            var e = new Event1();

            var h1 = Substitute.For<IDomainEventHandler<Event1>>();
            var h2 = Substitute.For<IDomainEventHandler<Event1>>();
            var fail = new FailingHandler<Event1>();

            await Prepare(h1, fail, h2).HandleEventsOf(Publish(e));

            _ = h1.Received(1).HandleAsync(e);
            _ = h2.Received(1).HandleAsync(e);
        }

        [LongRunningFact]
        public async Task If_handler_fails_its_events_are_ignored()
        {
            var h = Substitute.For<IDomainEventHandler<Event2>>();
            var fail = new FailingHandler<Event1>(new Event2());

            await Prepare(h, fail).HandleEventsOf(Publish(new Event1()));

            _ = h.DidNotReceiveWithAnyArgs().HandleAsync(null);
        }

        [LongRunningFact]
        public async Task If_command_fails_The_call_fails()
        {
            await Assert.ThrowsAsync<Exception>(() =>
                Prepare().HandleEventsOf(() => { throw new Exception(); })
            );
        }

        [LongRunningFact]
        public async Task If_command_fails_Its_events_are_ignored()
        {
            var h = Substitute.For<IDomainEventHandler<Event1>>();
            await Assert.ThrowsAsync<Exception>(() =>
                Prepare(h).HandleEventsOf(() =>
                {
                    DomainEvents.Raise(new Event1());
                    throw new Exception();
                })
            );

            _ = h.DidNotReceiveWithAnyArgs().HandleAsync(null);
        }

        private static PipelineExecutor<Context, Func<int>, int> Prepare(params object[] handlers)
        {
            var resolver = new HandlerResolver(handlers);
            var interp = new EventsInterceptorElement<Context, Func<int>, int>(interceptor);
            var exec = new EventsExecutorElement<Context, Func<int>, int>(policies, interceptor, resolver);

            var scope = Substitute.For<IPipelineScope>();
            scope.ResolveElement<Context, Func<int>, int>(interp.GetType()).Returns(interp);
            scope.ResolveElement<Context, Func<int>, int>(exec.GetType()).Returns(exec);
            scope.ResolveFinalizer<Context, Func<int>, int>(null).ReturnsForAnyArgs(new ExecFinalizer());

            var factory = Substitute.For<IPipelineFactory>();
            factory.BeginScope().Returns(scope);

            var cfg = Pipeline.Build<Context, Func<int>, int>()
                .Use<EventsExecutorElement<Context, Func<int>, int>>()
                .Use<EventsInterceptorElement<Context, Func<int>, int>>()
                .Finalize<ExecFinalizer>();
            return PipelineExecutor.Create(factory, cfg);
        }

        private static Func<int> Publish(
            params IDomainEvent[] events)
        {
            return () =>
            {
                foreach (var e in events)
                {
                    DomainEvents.Raise(e);
                }
                return 1;
            };
        }
    }
}
