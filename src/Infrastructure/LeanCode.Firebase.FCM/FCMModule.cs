using Autofac;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using LeanCode.Components;

namespace LeanCode.Firebase.FCM
{
    public class FCMModule : AppModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => FirebaseMessaging.GetMessaging(c.Resolve<FirebaseApp>()))
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<FCMClient>()
                .AsSelf();
        }
    }
}
