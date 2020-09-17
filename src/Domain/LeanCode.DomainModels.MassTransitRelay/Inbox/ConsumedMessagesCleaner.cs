using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using LeanCode.Dapper;
using LeanCode.PeriodicService;
using LeanCode.Time;

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
            logger.Debug("Starting periodic message cleanup");
            var time = TimeProvider.Now - KeepTime;
            var deleted = await dbContext.Self.ExecuteScalarAsync<int>(
                $@"DELETE t FROM {tableName} t
                WHERE t.[DateConsumed] < @time;",
                new { time },
                commandTimeout: 3600);
            logger.Information("Deleted {Count} consumed messages", deleted);
        }
    }
}
