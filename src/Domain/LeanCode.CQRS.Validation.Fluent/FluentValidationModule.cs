using Autofac;
using FluentValidation;
using LeanCode.Components;

namespace LeanCode.CQRS.Validation.Fluent
{
    class FluentValidationModule : Module
    {
        private readonly TypesCatalog catalog;

        public FluentValidationModule(TypesCatalog catalog)
        {
            this.catalog = catalog;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(AdapterLoader<>))
                .AsImplementedInterfaces()
                .SingleInstance();

            builder
                .RegisterAssemblyTypes(catalog.Assemblies)
                .AsClosedTypesOf(typeof(IValidator<>))
                .SingleInstance();
        }
    }
}
