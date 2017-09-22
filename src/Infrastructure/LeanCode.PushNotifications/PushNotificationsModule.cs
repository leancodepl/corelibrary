using Autofac;
using LeanCode.Configuration;
using Microsoft.Extensions.Configuration;

namespace LeanCode.PushNotifications
{
    class PushNotificationsModule<TUserId> : Module
    {
        private readonly IConfiguration config;

        public PushNotificationsModule(IConfiguration config)
        {
            this.config = config;
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (config != null)
            {
                builder.ConfigSection<FCMConfiguration>(config);
            }
            builder.RegisterType<PushNotifications<TUserId>>().As<IPushNotifications<TUserId>>();
            builder.RegisterType<FCMClient>().AsSelf().SingleInstance();
        }
    }
}
