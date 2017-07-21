using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using LeanCode.CQRS.Cache;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Validation;
using LeanCode.DomainModels.EventsExecution;
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

        public static DomainComponent WithDefaultPipelines(TypesCatalog catalog)
        {
            return new DomainComponent(catalog,
                b => b.Secure().Validate().ExecuteEvents().InterceptEvents(),
                b => b.Secure().Cache()
            );
        }
    }
}
