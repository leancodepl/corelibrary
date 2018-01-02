using Autofac;
using FluentValidation;
using LeanCode.Components;

namespace LeanCode.CQRS.Validation.Fluent
{
    public class FluentValidationModule : AppModule
    {
        private readonly TypesCatalog catalog;

        public FluentValidationModule(TypesCatalog catalog)
        {
            this.catalog = catalog;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(AdapterLoader<,,>))
                .AsImplementedInterfaces();

            builder
                .RegisterAssemblyTypes(catalog.Assemblies)
                .AsClosedTypesOf(typeof(IValidator<>))
                .SingleInstance();
        }
    }
}
