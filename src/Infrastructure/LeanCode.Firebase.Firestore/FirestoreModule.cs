using Autofac;
using LeanCode.Components;
using LeanCode.OrderedHostedServices;

namespace LeanCode.Firebase.Firestore
{
    public class FirestoreModule : AppModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterOrderedHostedService<FirestoreDatabase>()
                .AsSelf()
                .SingleInstance();
        }
    }
}
