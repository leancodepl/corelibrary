using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using LeanCode.Dapper;
using LeanCode.IdentityProvider;
using LeanCode.TimeProvider;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.Firebase.FCM.EntityFramework
{
    public sealed class PushNotificationTokenStore<TDbContext> : IPushNotificationTokenStore
        where TDbContext : DbContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PushNotificationTokenStore<TDbContext>>();

        private readonly TDbContext dbContext;

        public PushNotificationTokenStore(TDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<string>> GetTokensAsync(Guid userId)
        {
            var res = await dbContext.QueryAsync<string>(
                $"SELECT [Token] FROM {GetTokensTableName()} WHERE [UserId] = @userId",
                new { userId });
            return res.AsList();
        }

        public async Task AddUserTokenAsync(Guid userId, string token)
        {
            try
            {
                await dbContext.ExecuteAsync(
                $@"
                    BEGIN TRAN;

                    -- Remove token from (possibly another) user
                    DELETE FROM {GetTokensTableName()} WHERE [Token] = @token;

                    -- And add the new token
                    INSERT INTO {GetTokensTableName()} ([Id], [UserId], [Token], [DateCreated])
                    VALUES (@newId, @userId, @token, @now);

                    COMMIT TRAN;
                ", new { newId = Identity.NewId(), userId, token, now = Time.Now });
                logger.Information("Removed push notification token for user {UserId} from the store", userId);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Something went wrong when deleting push notification token for user {UserId}", userId);
                throw;
            }
        }

        public async Task RemoveUserTokenAsync(Guid userId, string token)
        {
            try
            {
                await dbContext.ExecuteAsync(
                    $"DELETE FROM {GetTokensTableName()} WHERE [UserId] = @userId AND [Token] = @token",
                    new { userId, token });
                logger.Information("Removed push notification token for user {UserId} from the store", userId);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Something went wrong when deleting push notification token for user {UserId}", userId);
            }
        }

        public async Task RemoveTokenAsync(string token)
        {
            try
            {
                await dbContext.ExecuteAsync(
                    $"DELETE FROM {GetTokensTableName()} WHERE [Token] = @token",
                    new { token });
                logger.Information("Removed push notification token from the store");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Something went wrong when deleting push notification token");
            }
        }

        public async Task RemoveTokensAsync(IEnumerable<string> tokens)
        {
            try
            {
                await dbContext.ExecuteAsync(
                    $"DELETE FROM {GetTokensTableName()} WHERE [Token] IN @tokens",
                    new { tokens });
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
    }
}
