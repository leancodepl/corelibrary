using Autofac;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using LeanCode.Components;

namespace LeanCode.Firebase.FCM;

public class FCMModule : AppModule
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(c => FirebaseMessaging.GetMessaging(c.Resolve<FirebaseApp>())).AsSelf().SingleInstance();

        builder.RegisterType<FCMClient>().AsSelf();
    }
}

public class FCMModule<TStore> : AppModule
    where TStore : class, IPushNotificationTokenStore
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule(new FCMModule());
        builder.RegisterType<TStore>().AsImplementedInterfaces();
    }
}
