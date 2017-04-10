using Autofac;
using Autofac.Features.Variance;
using LeanCode.Components;
using LeanCode.DomainModels.Model;
using Module = Autofac.Module;

namespace LeanCode.ComainModels.Autofac
{
    public class DomainModelsModule : Module
    {
        private readonly TypesCatalog catalog;

        public DomainModelsModule(TypesCatalog catalog)
        {
            this.catalog = catalog;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterSource(new ContravariantRegistrationSource());

            builder.RegisterType<AutofacEventHandlerResolver>().As<IDomainEventHandlerResolver>();
            builder.RegisterAssemblyTypes(catalog.Assemblies).AsClosedTypesOf(typeof(IDomainEventHandler<>));
        }
    }
}
