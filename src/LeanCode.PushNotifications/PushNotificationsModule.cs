using Autofac;

namespace LeanCode.PushNotifications
{
    class PushNotificationsModule<TUserId> : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PushNotifications<TUserId>>().As<IPushNotifications<TUserId>>();
            builder.RegisterType<FCMClient>().As<IFCMClient>().SingleInstance();
        }
    }
}
