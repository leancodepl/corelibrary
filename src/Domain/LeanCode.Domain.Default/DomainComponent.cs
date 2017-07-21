using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Domain.Default
{

    public class DomainComponent : IAppComponent
    {
        public IModule AutofacModule { get; }
        public Profile MapperProfile => null;

        public DomainComponent(
            TypesCatalog catalog,
            CommandBuilder cmdBuilder,
            QueryBuilder queryBuilder)
        {
            AutofacModule = new DomainModule(catalog, cmdBuilder, queryBuilder);
        }

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
