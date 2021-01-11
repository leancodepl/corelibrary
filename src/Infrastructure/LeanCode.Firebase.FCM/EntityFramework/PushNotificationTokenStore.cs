using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using LeanCode.Dapper;
using LeanCode.IdentityProvider;
using LeanCode.Time;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.Firebase.FCM.EntityFramework
{
    public sealed class PushNotificationTokenStore<TDbContext> : IPushNotificationTokenStore
        where TDbContext : DbContext
    {
        private const int MaxTokenBatchSize = IPushNotificationTokenStore.MaxTokenBatchSize;
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PushNotificationTokenStore<TDbContext>>();

        private readonly TDbContext dbContext;

        public PushNotificationTokenStore(TDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<string>> GetTokensAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var res = await dbContext.QueryAsync<string>(
                $"SELECT [Token] FROM {GetTokensTableName()} WHERE [UserId] = @userId",
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

            var res = await dbContext.QueryAsync<UserToken>(
                $"SELECT [UserId], [Token] FROM {GetTokensTableName()} WHERE [UserId] IN @userIds",
                new { userIds },
                cancellationToken: cancellationToken);
            return res
                .GroupBy(g => g.UserId)
                    .ToDictionary(
                        t => t.Key,
                        t => t.Select(e => e.Token).ToList());
        }

        public async Task AddUserTokenAsync(Guid userId, string token, CancellationToken cancellationToken = default)
        {
            try
            {
                await dbContext.ExecuteAsync(
                $@"
                    BEGIN TRANSACTION;

                    -- Remove token from (possibly another) user
                    DELETE FROM {GetTokensTableName()} WHERE [Token] = @token;

                    -- And add the new token
                    INSERT INTO {GetTokensTableName()} ([Id], [UserId], [Token], [DateCreated])
                    VALUES (@newId, @userId, @token, @now);

                    COMMIT TRANSACTION;
                ", new { newId = Identity.NewId(), userId, token, now = TimeProvider.Now },
                cancellationToken: cancellationToken);
                logger.Information("Added push notification token for user {UserId} from the store", userId);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Something went wrong when adding push notification token for user {UserId}", userId);
                throw;
            }
        }

        public async Task RemoveUserTokenAsync(Guid userId, string token, CancellationToken cancellationToken = default)
        {
            try
            {
                await dbContext.ExecuteAsync(
                    $"DELETE FROM {GetTokensTableName()} WHERE [UserId] = @userId AND [Token] = @token",
                    new { userId, token },
                    cancellationToken: cancellationToken);
                logger.Information("Removed push notification token for user {UserId} from the store", userId);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Something went wrong when deleting push notification token for user {UserId}", userId);
            }
        }

        public async Task RemoveTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            try
            {
                await dbContext.ExecuteAsync(
                    $"DELETE FROM {GetTokensTableName()} WHERE [Token] = @token",
                    new { token },
                    cancellationToken: cancellationToken);
                logger.Information("Removed push notification token from the store");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Something went wrong when deleting push notification token");
            }
        }

        public async Task RemoveTokensAsync(IEnumerable<string> tokens, CancellationToken cancellationToken = default)
        {
            try
            {
                await dbContext.ExecuteAsync(
                    $"DELETE FROM {GetTokensTableName()} WHERE [Token] IN @tokens",
                    new { tokens },
                    cancellationToken: cancellationToken);
                logger.Information("Removed {Count} push notification tokens from the store", tokens.Count());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Something went wrong when deleting push notification tokens");
            }
        }

        private string GetTokensTableName()
        {
            var entity = dbContext.Model.FindEntityType(typeof(PushNotificationTokenEntity));
            if (string.IsNullOrEmpty(entity.GetSchema()))
            {
                return $"[{entity.GetTableName()}]";
            }
            else
            {
                return $"[{entity.GetSchema()}].[{entity.GetTableName()}]";
            }
        }

        private readonly struct UserToken
        {
            public Guid UserId { get; }
            public string Token { get; }
        }
    }
}
