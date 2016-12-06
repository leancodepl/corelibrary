using System;
using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.FluentValidation
{
    public class FluentValidationApp : IAppComponent
    {
        public IModule AutofacModule { get; }
        public Profile MapperProfile => null;

        public FluentValidationApp(Type[] searchAssemblies)
        {
            AutofacModule = new FluentValidationModule(searchAssemblies);
        }

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
