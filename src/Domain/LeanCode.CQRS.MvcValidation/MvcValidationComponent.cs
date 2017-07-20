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

        public MvcValidationComponent(TypesCatalog catalog)
        {
            AutofacModule = new MvcValidationModule(catalog);
        }

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
