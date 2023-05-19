using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Firebase.Firestore;

public class FirestoreModule : AppModule
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.TryRegisterWithImplementedInterfaces<FirestoreDatabase>(ServiceLifetime.Singleton);
    }
}
