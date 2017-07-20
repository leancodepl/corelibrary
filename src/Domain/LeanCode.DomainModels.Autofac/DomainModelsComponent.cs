using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DomainModels.Autofac
{
    public class DomainModelsComponent : IAppComponent
    {
        public IModule AutofacModule { get; }
        public Profile MapperProfile => null;

        public DomainModelsComponent(TypesCatalog catalog)
        {
            AutofacModule = new DomainModelsModule(catalog);
        }

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
