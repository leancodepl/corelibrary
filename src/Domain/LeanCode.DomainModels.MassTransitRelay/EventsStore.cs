using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeanCode.DomainModels.MassTransitRelay.Outbox;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.MassTransitRelay;

public class EventsStore
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<EventsStore>();
    private readonly IRaisedEventsSerializer eventsSerializer;
    private readonly IOutboxContext outboxContext;

    public EventsStore(IOutboxContext outboxContext, IRaisedEventsSerializer eventsSerializer)
    {
        this.outboxContext = outboxContext;
        this.eventsSerializer = eventsSerializer;
    }

    public async Task StoreAndPublishEventsAsync(
        List<IDomainEvent> events,
        Guid? conversationId,
        IEventPublisher publisher,
        CancellationToken cancellationToken = default
    )
    {
        if (events?.Count > 0)
        {
            var persisted = PersistEvents(events, conversationId);
            await outboxContext.SaveChangesAsync(cancellationToken);

            var publishStatuses = await PublishEventsAsync(events, publisher, conversationId, cancellationToken);
            MarkEventsAsRaised(persisted, publishStatuses);
            await outboxContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            await outboxContext.SaveChangesAsync(cancellationToken);
        }
    }

    private List<RaisedEvent> PersistEvents(List<IDomainEvent> events, Guid? conversationId)
    {
        var evts = events
            .Select(evt =>
            {
                var context = Activity.Current?.Context;
                var meta = new RaisedEventMetadata { ConversationId = conversationId, ActivityContext = context, };

                return eventsSerializer.WrapEvent(evt, meta);
            })
            .ToList();

        outboxContext.RaisedEvents.AddRange(evts);
        return evts;
    }

    private static void MarkEventsAsRaised(List<RaisedEvent> events, bool[] statuses)
    {
        if (events.Count != statuses.Length)
        {
            throw new InvalidOperationException("Missing publish status");
        }

        for (var i = 0; i < events.Count; ++i)
        {
            events[i].WasPublished = statuses[i];
        }
    }

    private Task<bool[]> PublishEventsAsync(
        List<IDomainEvent> events,
        IEventPublisher publisher,
        Guid? conversationId,
        CancellationToken cancellationToken
    )
    {
        logger.Debug("Publishing {Count} raised events", events.Count);

        var publishTasks = events.Select(evt => PublishEventAsync(evt, publisher, conversationId, cancellationToken));

        return Task.WhenAll(publishTasks);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "?",
        "CA1031",
        Justification = "The method is an exception boundary."
    )]
    private async Task<bool> PublishEventAsync(
        IDomainEvent evt,
        IEventPublisher publisher,
        Guid? conversationId,
        CancellationToken cancellationToken
    )
    {
        logger.Debug("Publishing event of type {DomainEvent}", evt);

        try
        {
            await publisher.PublishAsync(evt, conversationId, cancellationToken);
            logger.Information("Domain event {DomainEvent} published", evt);
            return true;
        }
        catch (Exception e)
        {
            logger.Warning(e, "Could not publish event {@DomainEvent}", evt);
            return false;
        }
    }
}
