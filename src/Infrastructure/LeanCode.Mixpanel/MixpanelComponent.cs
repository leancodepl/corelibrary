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
        public IModule AutofacModule { get; }
        public Profile MapperProfile { get; }

        public void ConfigureServices(IServiceCollection services)
        { }

        public MixpanelComponent(IConfigurationRoot configuration)
        {
            AutofacModule = new MixpanelModule(configuration);
        }
    }
}
