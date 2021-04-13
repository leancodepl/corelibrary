using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using LeanCode.Dapper;
using LeanCode.OpenTelemetry;
using LeanCode.PeriodicService;
using LeanCode.Time;
using OpenTelemetry.Trace;

namespace LeanCode.DomainModels.MassTransitRelay.Inbox
{
    public class ConsumedMessagesCleaner : IPeriodicAction
    {
        private static readonly TimeSpan KeepTime = TimeSpan.FromDays(3);

        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<ConsumedMessagesCleaner>();

        public CronExpression When { get; } = CronExpression.Parse("0 2 * * *");
        public bool SkipFirstExecution => false;

        private readonly IConsumedMessagesContext dbContext;
        private readonly string tableName;

        public ConsumedMessagesCleaner(IConsumedMessagesContext dbContext)
        {
            this.dbContext = dbContext;
            tableName = dbContext.Self.GetFullTableName(typeof(ConsumedMessage));
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var activity = LeanCodeActivitySource.Start("inbox.clear");

            try
            {
                logger.Verbose("Starting periodic message cleanup");
                var time = TimeProvider.Now - KeepTime;

                var deleted = await dbContext.Self.ExecuteScalarAsync<int>(
                    $@"
                    DELETE FROM {tableName}
                    WHERE [DateConsumed] < @time;",
                    new { time },
                    commandTimeout: 3600,
                    cancellationToken: stoppingToken);
                logger.Verbose("Deleted {Count} consumed messages", deleted);
            }
            catch
            {
                activity.SetStatus(Status.Error);
                throw;
            }
        }
    }
}
