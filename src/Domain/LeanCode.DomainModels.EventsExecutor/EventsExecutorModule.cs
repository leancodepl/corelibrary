using Autofac;
using Autofac.Features.Variance;
using LeanCode.Components;

namespace LeanCode.DomainModels.EventsExecutor
{
    class EventsExecutorModule : Module
    {
        private readonly TypesCatalog catalog;

        public EventsExecutorModule(TypesCatalog catalog)
        {
            this.catalog = catalog;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AsyncEventsInterceptor>()
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<RetryPolicies>().AsSelf().SingleInstance();

            builder.RegisterSource(new ContravariantRegistrationSource());

            builder.RegisterType<AutofacEventHandlerResolver>().As<IDomainEventHandlerResolver>();
            builder.RegisterAssemblyTypes(catalog.Assemblies).AsClosedTypesOf(typeof(IDomainEventHandler<>));
        }
    }
}
