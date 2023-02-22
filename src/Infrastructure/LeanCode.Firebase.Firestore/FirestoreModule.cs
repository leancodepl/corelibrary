using Autofac;
using LeanCode.Components;

namespace LeanCode.Firebase.Firestore;

public class FirestoreModule : AppModule
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<FirestoreDatabase>().AsSelf().AsImplementedInterfaces().SingleInstance();
    }
}
