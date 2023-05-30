using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeanCode.Firebase.FCM.PostgreSql;

public static class FCMBuilderExtensions
{
    public static void AddPostgreSqlTokenStore<TDbContext>(this FCMBuilder builder)
        where TDbContext : DbContext
    {
        builder.Services.TryAddTransient<IPushNotificationTokenStore, PgSqlPushNotificationTokenStore<TDbContext>>();
    }
}
