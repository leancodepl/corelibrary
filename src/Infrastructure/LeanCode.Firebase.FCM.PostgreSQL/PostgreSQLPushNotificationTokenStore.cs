using Dapper;
using LeanCode.Dapper;
using LeanCode.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace LeanCode.Firebase.FCM.PostgreSQL
{
    public sealed class PostgreSQLPushNotificationTokenStore<TDbContext> : IPushNotificationTokenStore
        where TDbContext : DbContext
    {
        private const int MaxTokenBatchSize = IPushNotificationTokenStore.MaxTokenBatchSize;
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PostgreSQLPushNotificationTokenStore<TDbContext>>();

        private readonly TDbContext dbContext;
        private readonly ISqlGenerationHelper sqlGenerationHelper;

        public PostgreSQLPushNotificationTokenStore(TDbContext dbContext)
        {
            this.dbContext = dbContext;

            sqlGenerationHelper = dbContext.GetService<ISqlGenerationHelper>();
        }

        public async Task<List<string>> GetTokensAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var res = await dbContext.QueryAsync<string>(
                $"SELECT {GetTokensColumnName(nameof(PostgreSQLPushNotificationEntity.Token))} FROM {GetTokensTableName()} WHERE {GetTokensColumnName(nameof(PostgreSQLPushNotificationEntity.UserId))} = @userId",
                new { userId },
                cancellationToken: cancellationToken);
            return res.AsList();
        }

        public async Task<Dictionary<Guid, List<string>>> GetTokensAsync(IReadOnlySet<Guid> userIds, CancellationToken cancellationToken = default)
        {
            if (userIds.Count > MaxTokenBatchSize)
            {
                throw new ArgumentException($"You can only pass at most {MaxTokenBatchSize} users in one call.", nameof(userIds));
            }

            var userIdsList = userIds.ToList();

            var res = await dbContext.QueryAsync<UserToken>(
                $"SELECT {GetTokensColumnName(nameof(PostgreSQLPushNotificationEntity.UserId))}, {GetTokensColumnName(nameof(PostgreSQLPushNotificationEntity.Token))} FROM {GetTokensTableName()} WHERE {GetTokensColumnName(nameof(PostgreSQLPushNotificationEntity.UserId))} = ANY(@userIdsList)",
                new { userIdsList },
                cancellationToken: cancellationToken);
            return res
                .GroupBy(g => g.UserId)
                .ToDictionary(
                    t => t.Key,
                    t => t.Select(e => e.Token).ToList());
        }

        public async Task AddUserTokenAsync(Guid userId, string newToken, CancellationToken cancellationToken = default)
        {
            try
            {
                await dbContext.ExecuteAsync(
                $@"
                    BEGIN TRANSACTION;

                    -- Remove token from (possibly another) user
                    DELETE FROM {GetTokensTableName()} WHERE {GetTokensColumnName(nameof(PostgreSQLPushNotificationEntity.Token))} = @newToken;

                    -- And add the new token
                    INSERT INTO {GetTokensTableName()}
                        ({GetTokensColumnName(nameof(PostgreSQLPushNotificationEntity.Id))},
                        {GetTokensColumnName(nameof(PostgreSQLPushNotificationEntity.UserId))},
                        {GetTokensColumnName(nameof(PostgreSQLPushNotificationEntity.Token))},
                        {GetTokensColumnName(nameof(PostgreSQLPushNotificationEntity.DateCreated))})
                    VALUES (@newId, @userId, @newToken, @now);

                    COMMIT TRANSACTION;
                ", new { newId = Guid.NewGuid(), userId, newToken, now = TimeProvider.Now },
                cancellationToken: cancellationToken);
                logger.Information("Added push notification token for user {UserId} from the store", userId);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Something went wrong when adding push notification token for user {UserId}", userId);
                throw;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1031", Justification = "The method is an exception boundary.")]
        public async Task RemoveUserTokenAsync(Guid userId, string newToken, CancellationToken cancellationToken = default)
        {
            try
            {
                await dbContext.ExecuteAsync(
                    $"DELETE FROM {GetTokensTableName()} WHERE {GetTokensColumnName(nameof(PostgreSQLPushNotificationEntity.UserId))} = @userId AND {GetTokensColumnName(nameof(PostgreSQLPushNotificationEntity.Token))} = @newToken",
                    new { userId, newToken },
                    cancellationToken: cancellationToken);
                logger.Information("Removed push notification token for user {UserId} from the store", userId);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Something went wrong when deleting push notification token for user {UserId}", userId);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1031", Justification = "The method is an exception boundary.")]
        public async Task RemoveTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            try
            {
                await dbContext.ExecuteAsync(
                    $"DELETE FROM {GetTokensTableName()} WHERE {GetTokensColumnName(nameof(PostgreSQLPushNotificationEntity.Token))} = @token",
                    new { token },
                    cancellationToken: cancellationToken);
                logger.Information("Removed push notification token from the store");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Something went wrong when deleting push notification token");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1031", Justification = "The method is an exception boundary.")]
        public async Task RemoveTokensAsync(IEnumerable<string> tokens, CancellationToken cancellationToken = default)
        {
            try
            {
                var tokensList = tokens.ToList();
                await dbContext.ExecuteAsync(
                    $"DELETE FROM {GetTokensTableName()} WHERE {GetTokensColumnName(nameof(PostgreSQLPushNotificationEntity.Token))} = ANY(@tokensList)",
                    new { tokensList },
                    cancellationToken: cancellationToken);
                logger.Information("Removed {Count} push notification tokens from the store", tokens.Count());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Something went wrong when deleting push notification tokens");
            }
        }

        private string GetTokensTableName() =>
            dbContext.GetFullTableName(typeof(PostgreSQLPushNotificationEntity));

        private string GetTokensColumnName(string propertyName) =>
            dbContext.GetColumnName(typeof(PostgreSQLPushNotificationEntity), propertyName);

        private readonly struct UserToken
        {
            public Guid UserId { get; }
            public string Token { get; }
        }
    }
}
