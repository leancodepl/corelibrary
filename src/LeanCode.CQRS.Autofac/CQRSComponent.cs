using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.Autofac
{
    public class CQRSComponent : IAppComponent
    {
        public IModule AutofacModule { get; }
        public Profile MapperProfile => null;

        public CQRSComponent(TypesCatalog catalog)
        {
            AutofacModule = new CQRSModule(catalog);
        }

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
