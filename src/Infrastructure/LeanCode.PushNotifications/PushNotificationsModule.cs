using System;
using Autofac;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.PushNotifications
{
    public class PushNotificationsModule<TUserId> : AppModule
        where TUserId : notnull
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient<FCMClient>()
                .ConfigureHttpClient((sp, c) =>
                {
                    var cfg = sp.GetService<FCMConfiguration>();
                    c.BaseAddress = new Uri("https://fcm.googleapis.com/fcm/send");
                    c.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "key=" + cfg.ApiKey);
                });
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PushNotifications<TUserId>>()
                .As<IPushNotifications<TUserId>>();
        }
    }
}
