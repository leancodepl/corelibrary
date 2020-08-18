using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using LeanCode.Dapper;
using LeanCode.OpenTelemetry;
using LeanCode.PeriodicService;
using LeanCode.Time;

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

        public PublishedEventsCleaner(IOutboxContext outboxContext)
        {
            this.outboxContext = outboxContext;
            tableName = outboxContext.Self.GetFullTableName(typeof(RaisedEvent));
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var activity = LeanCodeActivitySource.Start("outbox.clean");

            logger.Verbose("Startic raised events cleanup");
            var time = TimeProvider.Now - KeepPeriod;

            var count = await outboxContext.Self.ExecuteScalarAsync<int>(
                $@"DELETE t FROM {tableName} t WHERE
                t.[DateOcurred] < @time AND t.[WasPublished] = 1",
                new { time },
                commandTimeout: 3600,
                cancellationToken: stoppingToken);

            logger.Verbose("Removed {Count} raised and published events", count);
        }
    }
}
