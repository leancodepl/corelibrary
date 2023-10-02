using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeanCode.Firebase.FCM;

public static class FCMServiceCollectionExtensions
{
    public static IServiceCollection AddFCM<TUserId>(
        this IServiceCollection services,
        Action<FCMBuilder<TUserId>> config
    )
        where TUserId : notnull, IEquatable<TUserId>
    {
        services.TryAddSingleton(s => FirebaseMessaging.GetMessaging(s.GetRequiredService<FirebaseApp>()));
        services.TryAddTransient<FCMClient<TUserId>>();

        config(new FCMBuilder<TUserId>(services));
        return services;
    }
}

public class FCMBuilder<TUserId>
    where TUserId : notnull, IEquatable<TUserId>
{
    public IServiceCollection Services { get; }

    public FCMBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public void AddTokenStore<TDbContext>()
        where TDbContext : DbContext
    {
        Services.TryAddTransient<
            IPushNotificationTokenStore<TUserId>,
            PushNotificationTokenStore<TDbContext, TUserId>
        >();
    }
}
