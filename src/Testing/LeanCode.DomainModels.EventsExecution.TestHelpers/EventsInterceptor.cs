using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using LeanCode.DomainModels.Model;

namespace LeanCode.UnitTests.TestHelpers;

public static class EventsInterceptor
{
    private static readonly TestDomainEventInterceptor TestInterceptor = new TestDomainEventInterceptor();

    public static void Configure()
    {
        var existing = DomainEvents.EventInterceptor;

        if (existing is null || existing == TestInterceptor)
        {
            DomainEvents.SetInterceptor(TestInterceptor);
        }
        else
        {
            throw new InvalidOperationException(
                "Cannot use EventInterceptor when other interceptor is already configured.");
        }
    }

    public static SingleStorage<TEvent> Single<TEvent>()
        where TEvent : class, IDomainEvent
    {
        var eventStorage = new SingleStorage<TEvent>();

        TestInterceptor.AddHandler(eventStorage.Store);

        return eventStorage;
    }

    public static AllStorage<TEvent> All<TEvent>()
        where TEvent : class, IDomainEvent
    {
        var eventStorage = new AllStorage<TEvent>();

        TestInterceptor.AddHandler(eventStorage.Store);

        return eventStorage;
    }

    private sealed class TestDomainEventInterceptor : IDomainEventInterceptor
    {
        private readonly AsyncLocal<ConcurrentBag<Action<IDomainEvent>>?> handlers
            = new AsyncLocal<ConcurrentBag<Action<IDomainEvent>>?>();

        public void AddHandler(Action<IDomainEvent> func) =>
            (handlers.Value ??= new ConcurrentBag<Action<IDomainEvent>>()).Add(func);

        public void Intercept(IDomainEvent domainEvent)
        {
            foreach (var handler in handlers.Value?.ToArray() ?? Array.Empty<Action<IDomainEvent>>())
            {
                handler.Invoke(domainEvent);
            }
        }
    }
}

public sealed class SingleStorage<TEvent>
    where TEvent : class, IDomainEvent
{
    public bool Raised { get; private set; }
    public TEvent? Event { get; private set; }

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
    where TEvent : class, IDomainEvent
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
