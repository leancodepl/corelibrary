using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace LeanCode.Firebase.Firestore;

public static class FirestoreServiceProviderExtensions
{
    public static void AddFirestore(this IServiceCollection services)
    {
        services.TryAddSingleton<FirestoreDatabase>();
        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<FirestoreDatabase>());
    }
}
