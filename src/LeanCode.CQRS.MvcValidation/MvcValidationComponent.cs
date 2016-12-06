using System;
using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.MvcValidation
{
    public class MvcValidationComponent : IAppComponent
    {
        public Profile MapperProfile => null;
        public IModule AutofacModule { get; }

        public MvcValidationComponent(Type[] searchAssemblies)
        {
            AutofacModule = new MvcValidationModule(searchAssemblies);
        }

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
