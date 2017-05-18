using System;
using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.Configuration;

namespace LeanCode.DataStorage
{
    public class DataStorageComponent : IAppComponent
    {
        public IModule AutofacModule { get; }
        public Profile MapperProfile { get; }

        public DataStorageComponent(IConfiguration config)
        {
            AutofacModule = new DataStorageModule(config);
        }

        public void ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        { }
    }
}
