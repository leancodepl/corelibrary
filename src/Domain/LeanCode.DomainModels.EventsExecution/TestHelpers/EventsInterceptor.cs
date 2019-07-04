using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using LeanCode.DomainModels.Model;

namespace LeanCode.UnitTests.TestHelpers
{
    public static class EventsInterceptor
    {
        private static readonly TestDomainEventInterceptor testInterceptor = new TestDomainEventInterceptor();

        public static void Configure()
        {
            var existing = DomainEvents.EventInterceptor;
            if (!(existing is null) &&
                existing != testInterceptor)
            {
                throw new InvalidOperationException("Cannot use EventInterceptor when other interceptor is already configured.");
            }
            DomainEvents.SetInterceptor(testInterceptor);
        }

        public static SingleStorage<TEvent> Single<TEvent>()
            where TEvent : IDomainEvent
        {
            var eventStorage = new SingleStorage<TEvent>();
            testInterceptor.AddHandler(eventStorage.Store);
            return eventStorage;
        }

        public static AllStorage<TEvent> All<TEvent>()
            where TEvent : IDomainEvent
        {
            var eventStorage = new AllStorage<TEvent>();
            testInterceptor.AddHandler(eventStorage.Store);
            return eventStorage;
        }

        public sealed class SingleStorage<TEvent>
            where TEvent : IDomainEvent
        {
            public bool Raised { get; private set; } = false;
            public TEvent Event { get; private set; } = default;

            internal void Store(IDomainEvent @event)
            {
                if (@event is TEvent ev)
                {
                    Raised = true;
                    Event = ev;
                }
            }
        }

        public sealed class AllStorage<TEvent>
            where TEvent : IDomainEvent
        {
            private readonly List<TEvent> events = new List<TEvent>();

            public bool Raised { get; private set; }
            public IReadOnlyList<TEvent> Events => events;

            internal void Store(IDomainEvent @event)
            {
                if (@event is TEvent ev)
                {
                    Raised = true;
                    events.Add(ev);
                }
            }
        }

        private sealed class TestDomainEventInterceptor : IDomainEventInterceptor
        {
            private static readonly Action<IDomainEvent>[] EmptyHandlers = new Action<IDomainEvent>[0];
            private readonly AsyncLocal<ConcurrentBag<Action<IDomainEvent>>> handlers = new AsyncLocal<ConcurrentBag<Action<IDomainEvent>>>();

            public void AddHandler(Action<IDomainEvent> func)
            {
                if (handlers.Value == null)
                {
                    handlers.Value = new ConcurrentBag<Action<IDomainEvent>>();
                }

                handlers.Value.Add(func);
            }

            public void Intercept(IDomainEvent domainEvent)
            {
                var b = handlers.Value?.ToArray() ?? EmptyHandlers;
                foreach (var h in b)
                {
                    h.Invoke(domainEvent);
                }
            }
        }
    }
}
