using System;
using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.FluentValidation
{
    public class FluentValidationComponent : IAppComponent
    {
        public IModule AutofacModule { get; }
        public Profile MapperProfile => null;

        public FluentValidationComponent(Type[] searchAssemblies)
        {
            AutofacModule = new FluentValidationModule(searchAssemblies);
        }

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
