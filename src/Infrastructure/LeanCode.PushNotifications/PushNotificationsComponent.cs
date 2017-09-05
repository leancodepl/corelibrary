using System;
using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.Configuration;

namespace LeanCode.PushNotifications
{
    public class PushNotificationsComponent<TUserId> : IAppComponent
    {
        public IModule AutofacModule { get; }
        public Profile MapperProfile { get; }

        private PushNotificationsComponent(IConfiguration config)
        {
            AutofacModule = new PushNotificationsModule<TUserId>(config);
        }

        public void ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        { }

        public static PushNotificationsComponent<TUserId> WithoutConfiguration() => new PushNotificationsComponent<TUserId>(null);
        public static PushNotificationsComponent<TUserId> WithConfiguration(IConfiguration config) => new PushNotificationsComponent<TUserId>(config);
    }
}
