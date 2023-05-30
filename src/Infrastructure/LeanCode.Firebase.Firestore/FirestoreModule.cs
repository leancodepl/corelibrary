using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace LeanCode.Firebase.Firestore;

public class FirestoreModule : AppModule
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.TryAddSingleton<FirestoreDatabase>();
        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<FirestoreDatabase>());
    }
}
