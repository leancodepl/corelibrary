using Dapper;
using LeanCode.Dapper;
using LeanCode.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace LeanCode.Firebase.FCM.PostgreSql;

public sealed class PgSqlPushNotificationTokenStore<TDbContext> : IPushNotificationTokenStore
    where TDbContext : DbContext
{
    private const int MaxTokenBatchSize = IPushNotificationTokenStore.MaxTokenBatchSize;
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PgSqlPushNotificationTokenStore<TDbContext>>();

    private readonly TDbContext dbContext;
    private readonly ISqlGenerationHelper sqlGenerationHelper;

    public PgSqlPushNotificationTokenStore(TDbContext dbContext)
    {
        this.dbContext = dbContext;

        sqlGenerationHelper = dbContext.GetService<ISqlGenerationHelper>();
    }

    public async Task<List<string>> GetTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var res = await dbContext.QueryAsync<string>(
            $"SELECT {GetTokensColumnName(nameof(PgSqlPushNotificationEntity.Token))} FROM {GetTokensTableName()} WHERE {GetTokensColumnName(nameof(PgSqlPushNotificationEntity.UserId))} = @userId",
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
            $"SELECT {GetTokensColumnName(nameof(PgSqlPushNotificationEntity.UserId))}, {GetTokensColumnName(nameof(PgSqlPushNotificationEntity.Token))} FROM {GetTokensTableName()} WHERE {GetTokensColumnName(nameof(PgSqlPushNotificationEntity.UserId))} = ANY(@userIdsList)",
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
                    DELETE FROM {GetTokensTableName()} WHERE {GetTokensColumnName(nameof(PgSqlPushNotificationEntity.Token))} = @newToken;

                    -- And add the new token
                    INSERT INTO {GetTokensTableName()}
                        ({GetTokensColumnName(nameof(PgSqlPushNotificationEntity.Id))},
                        {GetTokensColumnName(nameof(PgSqlPushNotificationEntity.UserId))},
                        {GetTokensColumnName(nameof(PgSqlPushNotificationEntity.Token))},
                        {GetTokensColumnName(nameof(PgSqlPushNotificationEntity.DateCreated))})
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
                $"DELETE FROM {GetTokensTableName()} WHERE {GetTokensColumnName(nameof(PgSqlPushNotificationEntity.UserId))} = @userId AND {GetTokensColumnName(nameof(PgSqlPushNotificationEntity.Token))} = @newToken",
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
                $"DELETE FROM {GetTokensTableName()} WHERE {GetTokensColumnName(nameof(PgSqlPushNotificationEntity.Token))} = @token",
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
                $"DELETE FROM {GetTokensTableName()} WHERE {GetTokensColumnName(nameof(PgSqlPushNotificationEntity.Token))} = ANY(@tokensList)",
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
        dbContext.GetFullTableName(typeof(PgSqlPushNotificationEntity));

    private string GetTokensColumnName(string propertyName) =>
        dbContext.GetColumnName(typeof(PgSqlPushNotificationEntity), propertyName);

    private readonly struct UserToken
    {
        public Guid UserId { get; }
        public string Token { get; }
    }
}
