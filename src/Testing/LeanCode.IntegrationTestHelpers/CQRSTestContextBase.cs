using System.Collections.Generic;
using LeanCode.Components;
using LeanCode.CQRS.Default;
using LeanCode.CQRS.Validation;
using LeanCode.DomainModels.EventsExecution;

namespace LeanCode.IntegrationTestHelpers
{
    public abstract class CQRSTestContextBase : IntegrationTestContextBase
    {
        protected override IEnumerable<IAppModule> CreateAppModules()
        {
            return new IAppModule[]
            {
                ConfigureCQRS(new CQRSModule()),
            };
        }

        protected abstract CQRSModule ConfigureCQRS(CQRSModule module);
    }

    public class CQRSTestContextBase<TAppContext>
        : CQRSTestContextBase
        where TAppContext : IEventsContext
    {
        protected override CQRSModule ConfigureCQRS(CQRSModule module) =>
            module.WithTestPipelines<TAppContext>();
    }

    public static class CQRSTestExtensions
    {
        public static CQRSModule WithTestPipelines<TAppContext>(
            this CQRSModule mod,
            TypesCatalog handlersCatalog)
            where TAppContext : IEventsContext
        {
            return mod.WithCustomPipelines<TAppContext>(
                handlersCatalog,
                b => b.Validate().ExecuteEvents().InterceptEvents(),
                b => b);
        }

        public static CQRSModule WithTestPipelines<TAppContext>(
            this CQRSModule mod)
            where TAppContext : IEventsContext
        {
            var catalog = new TypesCatalog(typeof(TAppContext));
            return mod.WithTestPipelines<TAppContext>(catalog);
        }
    }
}
