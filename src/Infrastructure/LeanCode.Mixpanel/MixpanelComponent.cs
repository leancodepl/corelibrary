using System;
using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Mixpanel
{
    public class MixpanelComponent : IAppComponent
    {
        public IModule AutofacModule { get; private set; }
        public Profile MapperProfile { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        { }

        private MixpanelComponent()
        { }

        private MixpanelComponent(IConfigurationRoot configuration)
        {
            AutofacModule = new MixpanelModule(configuration);
        }

        public static MixpanelComponent WithFactory()
        {
            return new MixpanelComponent()
            {
                AutofacModule = new MixpanelWithFactoryModule()
            };
        }
        public static MixpanelComponent WithConfiguration(IConfigurationRoot configuration)
            => new MixpanelComponent(configuration);

        public static MixpanelComponent WithoutConfiguration()
            => new MixpanelComponent(null);
    }
}
