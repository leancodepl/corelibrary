using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeanCode.DomainModels.MassTransitRelay.Outbox;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.MassTransitRelay
{
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
            Guid correlationId,
            IEventPublisher publisher,
            CancellationToken cancellationToken = default)
        {
            if (events?.Count > 0)
            {
                var persisted = PersistEvents(events, correlationId);
                await outboxContext.SaveChangesAsync(cancellationToken);

                var publishStatuses = await PublishEventsAsync(events, correlationId, publisher, cancellationToken);
                MarkEventsAsRaised(persisted, publishStatuses);
                await outboxContext.SaveChangesAsync(cancellationToken);
            }
            else
            {
                await outboxContext.SaveChangesAsync(cancellationToken);
            }
        }

        private List<RaisedEvent> PersistEvents(List<IDomainEvent> events, Guid correlationId)
        {
            var evts = events
                .Select(evt => eventsSerializer.WrapEvent(evt, correlationId))
                .ToList();

            outboxContext.RaisedEvents.AddRange(evts);
            return evts;
        }

        private void MarkEventsAsRaised(List<RaisedEvent> events, bool[] statuses)
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
            Guid correlationId,
            IEventPublisher publisher,
            CancellationToken cancellationToken)
        {
            logger.Debug("Publishing {Count} raised events", events.Count);

            var publishTasks = events
                .Select(evt => PublishEventAsync(evt, correlationId, publisher, cancellationToken));

            return Task.WhenAll(publishTasks);
        }

        private async Task<bool> PublishEventAsync(
            IDomainEvent evt,
            Guid correlationId,
            IEventPublisher publisher,
            CancellationToken cancellationToken)
        {
            logger.Debug("Publishing event of type {DomainEvent}", evt);

            try
            {
                await publisher.PublishAsync(evt, correlationId, cancellationToken);
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
}
