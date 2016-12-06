using System;
using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.MvcValidation
{
    public class MvcValidationAdapterApp : IAppComponent
    {
        public Profile MapperProfile => null;
        public IModule AutofacModule { get; }

        public MvcValidationAdapterApp(Type[] searchAssemblies)
        {
            AutofacModule = new MvcValidationAdapterModule(searchAssemblies);
        }

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
