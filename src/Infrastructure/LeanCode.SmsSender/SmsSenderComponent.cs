using System;
using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.SmsSender
{
    public class SmsSenderComponent : IAppComponent
    {
        public IModule AutofacModule { get; }
        public Profile MapperProfile { get; }

        private SmsSenderComponent(IConfiguration config, bool useConfig)
        {
            if (useConfig && config == null)
            {
                throw new ArgumentNullException("Provide config when using configuration.", nameof(config));
            }
            else if (!useConfig && config != null)
            {
                throw new ArgumentNullException("Do not provide config, when config is not used.", nameof(config));
            }
            AutofacModule = new SmsSenderModule(config);
        }

        public void ConfigureServices(IServiceCollection services)
        { }

        public static SmsSenderComponent WithoutConfiguration() => new SmsSenderComponent(null, false);
        public static SmsSenderComponent WithConfiguration(IConfiguration config) => new SmsSenderComponent(config, true);
    }
}
