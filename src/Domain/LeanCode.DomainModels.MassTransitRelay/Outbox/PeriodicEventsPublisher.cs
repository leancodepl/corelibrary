using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using LeanCode.DomainModels.MassTransitRelay.Outbox;
using LeanCode.OpenTelemetry;
using LeanCode.PeriodicService;
using LeanCode.Time;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public class PeriodicEventsPublisher : IPeriodicAction
    {
        private static readonly TimeSpan RelayPeriod = TimeSpan.FromDays(1);
        private static readonly int MaxEventsToFetch = 1000;

        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PeriodicEventsPublisher>();
        private readonly IOutboxContext outboxContext;
        private readonly IRaisedEventsSerializer serializer;
        private readonly IEventPublisher eventPublisher;

        public CronExpression When => CronExpression.Parse("* * * * *");
        public bool SkipFirstExecution => false;

        public PeriodicEventsPublisher(
            IOutboxContext outboxContext,
            IEventPublisher eventPublisher,
            IRaisedEventsSerializer serializer)
        {
            this.eventPublisher = eventPublisher;
            this.serializer = serializer;
            this.outboxContext = outboxContext;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.Debug("Publishing unpublished events");

            var events = await FetchUnpublishedEventsAsync();

            logger.Debug("There are {EventsCount} events to be published", events.Count);

            foreach (var evt in events)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                using var activity = LeanCodeActivitySource.ActivitySource.StartActivity(
                    typeof(PeriodicEventsPublisher).FullName!,
                    ActivityKind.Internal,
                    evt.Metadata.ActivityContext ?? default);

                activity?.AddTag("event.id", evt.Id.ToString());
                activity?.AddTag("event.type", evt.EventType);

                try
                {
                    var deserialized = serializer.ExtractEvent(evt);
                    await eventPublisher.PublishAsync(deserialized, evt.Id, evt.Metadata.ConversationId, stoppingToken);

                    evt.WasPublished = true;
                    await outboxContext.SaveChangesAsync(stoppingToken);
                }
                catch (Exception e)
                {
                    logger.Warning(e, "Failed to publish event {MessageId}", evt.Id);
                }
            }
        }

        public Task<List<RaisedEvent>> FetchUnpublishedEventsAsync()
        {
            var after = TimeProvider.Now - RelayPeriod;
            return outboxContext.RaisedEvents
                .Where(evt => evt.DateOcurred > after && !evt.WasPublished)
                .OrderBy(evt => evt.DateOcurred)
                .Take(MaxEventsToFetch)
                .ToListAsync();
        }
    }
}
