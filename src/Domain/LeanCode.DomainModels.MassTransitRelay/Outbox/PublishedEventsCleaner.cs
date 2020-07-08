using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using LeanCode.Dapper;
using LeanCode.PeriodicService;
using LeanCode.TimeProvider;

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
            logger.Debug("Startic raised events cleanup");
            var time = Time.Now - KeepPeriod;

            var count = await outboxContext.Self.ExecuteScalarAsync<int>(
                $@"DELETE FROM {tableName} t WHERE
                t.[DateOcurred] < @time AND t.[WasPublished] = 1",
                new[] { time },
                commandTimeout: 3600);

            logger.Information("Removed {Count} raised and published events", count);
        }
    }
}
