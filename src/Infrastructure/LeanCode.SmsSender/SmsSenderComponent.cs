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

        public SmsSenderComponent(IConfigurationRoot configuration)
        {
            AutofacModule = new SmsSenderModule(configuration);
        }
        public void ConfigureServices(IServiceCollection services)
        { }

    }
}