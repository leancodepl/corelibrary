using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using LeanCode.DomainModels.MassTransitRelay.Outbox;
using LeanCode.PeriodicService;
using LeanCode.TimeProvider;
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

            var events = await FetchUnpublishedEvents();

            logger.Information("There are {EventsCount} to be published", events.Count);

            foreach (var evt in events)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    var deserialized = serializer.ExtractEvent(evt);
                    await eventPublisher.PublishAsync(deserialized, evt.Id, evt.CorrelationId, stoppingToken);

                    evt.WasPublished = true;
                    await outboxContext.SaveChangesAsync(stoppingToken);
                }
                catch (Exception e)
                {
                    logger.Warning(e, "Failed to publish event {MessageId}", evt.Id);
                }
            }
        }

        public Task<List<RaisedEvent>> FetchUnpublishedEvents()
        {
            var after = Time.Now - RelayPeriod;
            return outboxContext.RaisedEvents
                .Where(evt => evt.DateOcurred > after && !evt.WasPublished)
                .OrderBy(evt => evt.DateOcurred)
                .Take(MaxEventsToFetch)
                .ToListAsync();
        }
    }
}
