using System;
using Autofac.Core;
using AutoMapper;
using LeanCode.Components;

namespace LeanCode.PushNotifications
{
    public class PushNotificationsComponent<TUserId> : IAppComponent
    {
        public IModule AutofacModule { get; } = new PushNotificationsModule<TUserId>();
        public Profile MapperProfile { get; }

        public void ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        { }
    }

    public class PushNotificationsComponent : PushNotificationsComponent<Guid>
    { }
}
