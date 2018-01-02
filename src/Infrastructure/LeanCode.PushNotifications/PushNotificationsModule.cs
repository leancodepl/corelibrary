using Autofac;
using LeanCode.Components;

namespace LeanCode.PushNotifications
{
    public class PushNotificationsModule<TUserId> : AppModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PushNotifications<TUserId>>().As<IPushNotifications<TUserId>>();
            builder.RegisterType<FCMClient>().AsSelf().SingleInstance();
        }
    }
}
