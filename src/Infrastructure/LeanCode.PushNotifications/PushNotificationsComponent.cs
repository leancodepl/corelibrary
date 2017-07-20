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

        [Obsolete("Use WithConfiguration/WithoutConfiguration factory methods.")]
        public PushNotificationsComponent(IConfiguration config)
            : this(config, true)
        { }

        private PushNotificationsComponent(IConfiguration config, bool useConfig)
        {
            if (useConfig && config == null)
            {
                throw new ArgumentNullException("Provide config when using configuration.", nameof(config));
            }
            else if (!useConfig && config != null)
            {
                throw new ArgumentNullException("Do not provide config, when config is not used.", nameof(config));
            }
            AutofacModule = new PushNotificationsModule<TUserId>(config);
        }

        public void ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        { }

        public static PushNotificationsComponent<TUserId> WithoutConfiguration() => new PushNotificationsComponent<TUserId>(null, false);
        public static PushNotificationsComponent<TUserId> WithConfiguration(IConfiguration config) => new PushNotificationsComponent<TUserId>(config, true);
    }
}
