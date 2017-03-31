using System;
using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.ComainModels.Autofac
{
    public class DomainModelsComponent : IAppComponent
    {
        public IModule AutofacModule { get; }
        public Profile MapperProfile => null;

        public DomainModelsComponent(Type[] searchAssemblies)
        {
            AutofacModule = new DomainModelsModule(searchAssemblies);
        }

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
