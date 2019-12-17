using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly DbSet<PushNotificationTokenEntity> dbSet;

        public PushNotificationTokenStore(TDbContext dbContext)
        {
            this.dbContext = dbContext;

            dbSet = dbContext.Set<PushNotificationTokenEntity>();
        }

        public Task<List<string>> GetTokensAsync(Guid userId)
        {
            return dbSet
                .Where(e => e.UserId == userId)
                .Select(e => e.Token)
                .ToListAsync();
        }

        public async Task AddTokenAsync(Guid userId, string newToken)
        {
            try
            {
                dbSet.Add(new PushNotificationTokenEntity
                {
                    Id = Identity.NewId(),
                    UserId = userId,
                    Token = newToken,
                    DateCreated = Time.Now,
                });
                await dbContext.SaveChangesAsync();
                logger.Information("New push notification token for user {UserId} saved", userId);
            }
            catch (DbUpdateException exception) when (IsConcurrencyException(exception))
            {
                logger.Verbose("Duplicate token received for user {UserId}", userId);
            }
        }

        public async Task RemoveTokenAsync(string token)
        {
            var entity = await dbSet.FirstOrDefaultAsync(t => t.Token == token);
            if (entity is object)
            {
                dbSet.Remove(entity);
                try
                {
                    await dbContext.SaveChangesAsync();
                    logger.Information("Removed push notificatoin token for user {UserId}", entity.UserId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    logger.Information(
                        "The push notificatio token for user {UserId} has been already deleted",
                        entity.UserId);
                }
            }
        }

        public async Task RemoveTokensAsync(IEnumerable<string> tokens)
        {
            await dbContext.ExecuteAsync(
                $"DELETE FROM {GetTokensTable()} WHERE [Token] IN @tokens",
                new { tokens });
        }

        private string GetTokensTable()
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

        private static bool IsConcurrencyException(DbUpdateException exception)
        {
            return
                exception.InnerException is SqlException sqlException &&
                (sqlException.Number == 2601 || sqlException.Number == 2627);
        }
    }
}
