using Autofac;

namespace LeanCode.PushNotifications
{
    class PushNotificationsModule<TUserId> : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FCMPushNotifications<TUserId>>().As<IPushNotifications<TUserId>>().SingleInstance();
        }
    }
}
