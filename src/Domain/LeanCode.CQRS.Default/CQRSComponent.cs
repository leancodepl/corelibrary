using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using LeanCode.CQRS.Cache;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Validation;
using LeanCode.DomainModels.EventsExecution;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.Default
{
    public class CQRSComponent : IAppComponent
    {
        public IModule AutofacModule { get; }
        public Profile MapperProfile => null;

        public CQRSComponent(
            TypesCatalog catalog,
            CommandBuilder cmdBuilder,
            QueryBuilder queryBuilder)
        {
            AutofacModule = new CQRSModule(catalog, cmdBuilder, queryBuilder);
        }

        public void ConfigureServices(IServiceCollection services)
        { }

        public static CQRSComponent WithDefaultPipelines(TypesCatalog catalog)
        {
            return new CQRSComponent(catalog,
                b => b.Secure().Validate().ExecuteEvents().InterceptEvents(),
                b => b.Secure().Cache()
            );
        }
    }
}
