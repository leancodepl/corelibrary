using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeanCode.Firebase.FCM;

public class FCMModule : AppModule
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.TryAddSingleton(s => FirebaseMessaging.GetMessaging(s.GetRequiredService<FirebaseApp>()));

        services.TryAddTransient<FCMClient>();
    }
}

public class FCMModule<TStore> : AppModule
    where TStore : class, IPushNotificationTokenStore
{
    public override void ConfigureServices(IServiceCollection services)
    {
        (new FCMModule()).ConfigureServices(services);
        services.TryAddTransient(typeof(TStore));
    }
}
