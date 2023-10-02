using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace LeanCode.Firebase.FCM;

public sealed class PushNotificationTokenStore<TDbContext, TUserId> : IPushNotificationTokenStore<TUserId>
    where TDbContext : DbContext
    where TUserId : notnull, IEquatable<TUserId>
{
    private const int MaxTokenBatchSize = IPushNotificationTokenStore<TUserId>.MaxTokenBatchSize;
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PushNotificationTokenStore<TDbContext, TUserId>>();

    private readonly TDbContext dbContext;

    public PushNotificationTokenStore(TDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<List<string>> GetTokensAsync(TUserId userId, CancellationToken cancellationToken = default)
    {
        return dbContext
            .Set<PushNotificationTokenEntity<TUserId>>()
            .Where(e => (object)e.UserId == (object)userId)
            .Select(e => e.Token)
            .ToListAsync(cancellationToken);
    }

    public Task<Dictionary<TUserId, List<string>>> GetTokensAsync(
        IReadOnlySet<TUserId> userIds,
        CancellationToken cancellationToken = default
    )
    {
        if (userIds.Count > MaxTokenBatchSize)
        {
            throw new ArgumentException(
                $"You can only pass at most {MaxTokenBatchSize} users in one call.",
                nameof(userIds)
            );
        }

        return dbContext
            .Set<PushNotificationTokenEntity<TUserId>>()
            .Where(e => userIds.Contains(e.UserId))
            .GroupBy(g => g.UserId)
            .ToDictionaryAsync(t => t.Key, t => t.Select(e => e.Token).ToList(), cancellationToken);
    }

    public async Task AddUserTokenAsync(TUserId userId, string newToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var helper = dbContext.GetService<ISqlGenerationHelper>();
            var entityType =
                dbContext.Model.FindEntityType(typeof(PushNotificationTokenEntity<TUserId>))
                ?? throw new InvalidOperationException("Failed to find entity type.");
            var table =
                entityType.GetTableName()
                ?? throw new InvalidOperationException("Entity is not mapped to database table.");
            var storeObject = StoreObjectIdentifier.Table(table, entityType.GetSchema());

            var tokensTable = GetTokensTableName();
            var userIdColumn = GetTokensColumnName(nameof(PushNotificationTokenEntity<TUserId>.UserId));
            var tokenColumn = GetTokensColumnName(nameof(PushNotificationTokenEntity<TUserId>.Token));
            var dateCreatedColumn = GetTokensColumnName(nameof(PushNotificationTokenEntity<TUserId>.DateCreated));

            // perform an upsert using SQL merge, ensuring that given token is assigned to our user and no-one else
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                FormattableStringFactory.Create(
                    $$"""
                    MERGE INTO {{tokensTable}} "pt"
                    USING (
                        VALUES ({0}, {1}, {2})
                    ) AS "nt" ("UserId", "Token", "DateCreated")
                    ON "nt"."Token" = "pt".{{tokenColumn}}
                    WHEN MATCHED THEN
                        UPDATE SET {{userIdColumn}} = "nt"."UserId",
                                   {{dateCreatedColumn}} = "nt"."DateCreated"
                    WHEN NOT MATCHED THEN
                        INSERT ({{userIdColumn}}, {{tokenColumn}}, {{dateCreatedColumn}})
                        VALUES ("nt"."UserId", "nt"."Token", "nt"."DateCreated");
                    """,
                    userId,
                    newToken,
                    TimeProvider.Time.UtcNow
                ),
                cancellationToken
            );

            logger.Information("Added push notification token for user {UserId} to the store", userId);

            string GetTokensTableName()
            {
                return entityType.GetSchema() is string schema
                    ? $"{helper.DelimitIdentifier(schema)}.{helper.DelimitIdentifier(table)}"
                    : helper.DelimitIdentifier(table);
            }

            string GetTokensColumnName(string propertyName)
            {
                var property =
                    entityType.FindProperty(propertyName)
                    ?? throw new InvalidOperationException($"Property with name '{propertyName}' is not defined.");
                var column =
                    property.FindColumn(in storeObject)
                    ?? throw new InvalidOperationException(
                        $"Property with name '{propertyName}' is not mapped to a column."
                    );
                return helper.DelimitIdentifier(column.Name);
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Something went wrong when adding push notification token for user {UserId}", userId);
            throw;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "?",
        "CA1031",
        Justification = "The method is an exception boundary."
    )]
    public async Task RemoveUserTokenAsync(TUserId userId, string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var removed = await dbContext
                .Set<PushNotificationTokenEntity<TUserId>>()
                .Where(e => (object)e.UserId == (object)userId && e.Token == token)
                .ExecuteDeleteAsync(cancellationToken);

            if (removed == 0)
            {
                logger.Information("Could not find push notification token for user {UserId} to remove", userId);
            }
            else
            {
                logger.Information("Removed push notification token for user {UserId} from the store", userId);
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Something went wrong when deleting push notification token for user {UserId}", userId);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "?",
        "CA1031",
        Justification = "The method is an exception boundary."
    )]
    public async Task RemoveTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var removed = await dbContext
                .Set<PushNotificationTokenEntity<TUserId>>()
                .Where(e => e.Token == token)
                .ExecuteDeleteAsync(cancellationToken);

            if (removed == 0)
            {
                logger.Information("Could not find push notification token to remove");
            }
            else
            {
                logger.Information("Removed push notification token from the store");
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Something went wrong when deleting push notification token");
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "?",
        "CA1031",
        Justification = "The method is an exception boundary."
    )]
    public async Task RemoveTokensAsync(IEnumerable<string> tokens, CancellationToken cancellationToken = default)
    {
        try
        {
            var removed = await dbContext
                .Set<PushNotificationTokenEntity<TUserId>>()
                .Where(e => tokens.Contains(e.Token))
                .ExecuteDeleteAsync(cancellationToken);

            logger.Information("Removed {Count} push notification tokens from the store", removed);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Something went wrong when deleting push notification tokens");
        }
    }
}
