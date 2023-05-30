using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeanCode.Firebase.FCM.SqlServer;

public static class FCMBuilderExtensions
{
    public static void AddSqlServerTokenStore<TDbContext>(this FCMBuilder builder)
        where TDbContext : DbContext
    {
        builder.Services.TryAddTransient<IPushNotificationTokenStore, MsSqlPushNotificationTokenStore<TDbContext>>();
    }
}
