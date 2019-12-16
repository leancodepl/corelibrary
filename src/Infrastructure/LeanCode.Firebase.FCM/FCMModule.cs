using Autofac;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using LeanCode.Components;
using Microsoft.EntityFrameworkCore;

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

    public class FCMModule<TDbContext> : AppModule
        where TDbContext : DbContext
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new FCMModule());
            builder.RegisterType<EntityFramework.PushNotificationTokenStore<TDbContext>>()
                .AsImplementedInterfaces();
        }
    }
}
