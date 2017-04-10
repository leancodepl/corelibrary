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

        public FluentValidationComponent(TypesCatalog catalog)
        {
            AutofacModule = new FluentValidationModule(catalog);
        }

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
