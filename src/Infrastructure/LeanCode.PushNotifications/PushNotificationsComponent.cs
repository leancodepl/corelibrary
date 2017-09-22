using System;
using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.Configuration;

namespace LeanCode.PushNotifications
{
    /// <summary>
    /// Registers <see cref="PushNotifications{TUserId}" /> implementation of
    /// <see cref="IPushNotifications{TUserId}" /> that uses <see cref="FCMClient" />
    /// under the hood. It also registers <see cref="FCMClient" /> with
    /// necessary configuration. If needed, use directly.
    ///
    /// It requires a separate implementation of
    /// <see cref="IPushNotificationTokenStore{TUserId}" />.
    /// </summary>
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
