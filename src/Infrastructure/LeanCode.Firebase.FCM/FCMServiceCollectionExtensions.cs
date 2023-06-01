using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeanCode.Firebase.FCM;

public static class FCMServiceCollectionExtensions
{
    public static IServiceCollection AddFCM(this IServiceCollection services, Action<FCMBuilder> config)
    {
        services.TryAddSingleton(s => FirebaseMessaging.GetMessaging(s.GetRequiredService<FirebaseApp>()));
        services.TryAddTransient<FCMClient>();

        config(new FCMBuilder(services));
        return services;
    }
}

public class FCMBuilder
{
    public IServiceCollection Services { get; }

    public FCMBuilder(IServiceCollection services)
    {
        Services = services;
    }
}
