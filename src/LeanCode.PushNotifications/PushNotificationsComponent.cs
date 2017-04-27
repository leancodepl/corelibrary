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

        public PushNotificationsComponent(IConfiguration config)
        {
            AutofacModule = new PushNotificationsModule<TUserId>(config);
        }

        public void ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        { }
    }

    public class PushNotificationsComponent : PushNotificationsComponent<Guid>
    {
        public PushNotificationsComponent(IConfiguration config)
            : base(config)
        { }
    }
}
