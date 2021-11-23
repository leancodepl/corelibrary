using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using LeanCode.Dapper;
using LeanCode.OpenTelemetry;
using LeanCode.PeriodicService;
using LeanCode.Time;
using OpenTelemetry.Trace;

namespace LeanCode.DomainModels.MassTransitRelay.Outbox
{
    public class PublishedEventsCleaner : IPeriodicAction
    {
        public static readonly TimeSpan KeepPeriod = TimeSpan.FromDays(3);
        public CronExpression When { get; } = CronExpression.Parse("0 2 * * *");
        public bool SkipFirstExecution => false;

        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PublishedEventsCleaner>();
        private readonly IOutboxContext outboxContext;
        private readonly string tableName;
        private readonly string dateOccurredColumnName;
        private readonly string wasPublishedColumnName;

        public PublishedEventsCleaner(IOutboxContext outboxContext)
        {
            this.outboxContext = outboxContext;
            tableName = outboxContext.Self.GetFullTableName(typeof(RaisedEvent));
            dateOccurredColumnName = outboxContext.Self.GetColumnName(typeof(RaisedEvent), nameof(RaisedEvent.DateOcurred));
            wasPublishedColumnName = outboxContext.Self.GetColumnName(typeof(RaisedEvent), nameof(RaisedEvent.WasPublished));
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var activity = LeanCodeActivitySource.Start("outbox.clean");
            try
            {
                logger.Verbose("Startic raised events cleanup");
                var time = TimeProvider.Now - KeepPeriod;

                var count = await outboxContext.Self.ExecuteScalarAsync<int>(
                    $@"
                    DELETE FROM {tableName} WHERE
                        {dateOccurredColumnName} < @time AND {wasPublishedColumnName} = '1'",
                    new { time },
                    commandTimeout: 3600,
                    cancellationToken: stoppingToken);

                logger.Verbose("Removed {Count} raised and published events", count);
            }
            catch
            {
                activity?.SetStatus(Status.Error);
                throw;
            }
        }
    }
}
