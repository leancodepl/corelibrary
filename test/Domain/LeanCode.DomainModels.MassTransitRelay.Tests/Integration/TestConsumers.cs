using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;
using MassTransit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.Integration;

public class Event1Consumer : IConsumer<Event1>
{
    private readonly HandledEventsReporter<Event1> reporter;

    public Event1Consumer(HandledEventsReporter<Event1> reporter)
    {
        this.reporter = reporter;
    }

    public Task Consume(ConsumeContext<Event1> context)
    {
        reporter.ReportEvent(this, context);
        DomainEvents.Raise(new Event2());
        return Task.CompletedTask;
    }
}

public class Event2FirstConsumer : IConsumer<Event2>
{
    private readonly HandledEventsReporter<Event2> reporter;

    public Event2FirstConsumer(HandledEventsReporter<Event2> reporter)
    {
        this.reporter = reporter;
    }

    public Task Consume(ConsumeContext<Event2> context)
    {
        DomainEvents.Raise(new Event3());
        reporter.ReportEvent(this, context);
        return Task.CompletedTask;
    }
}

public class Event2SecondConsumer : IConsumer<Event2>
{
    private readonly HandledEventsReporter<Event2> reporter;

    public Event2SecondConsumer(HandledEventsReporter<Event2> reporter)
    {
        this.reporter = reporter;
    }

    public Task Consume(ConsumeContext<Event2> context)
    {
        reporter.ReportEvent(this, context);
        return Task.CompletedTask;
    }
}

public class Event3RetryingConsumer : IConsumer<Event3>
{
    private static readonly ConcurrentDictionary<Guid, bool> IsRetry = new ConcurrentDictionary<Guid, bool>();
    private readonly HandledEventsReporter<Event3> reporter;

    public Event3RetryingConsumer(HandledEventsReporter<Event3> reporter)
    {
        this.reporter = reporter;
    }

    public Task Consume(ConsumeContext<Event3> context)
    {
        var isRetry = IsRetry.AddOrUpdate(context.Message.Id, false, (_, _) => true);
        if (!isRetry)
        {
            throw new InvalidOperationException("This handler fails for testing purposes");
        }

        reporter.ReportEvent(this, context);
        return Task.CompletedTask;
    }
}
