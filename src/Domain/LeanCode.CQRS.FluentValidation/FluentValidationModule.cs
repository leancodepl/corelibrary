using Autofac;
using FluentValidation;
using LeanCode.Components;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS.FluentValidation
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
            builder.RegisterType<AutofacFluentValidatorResolver>().As<ICommandValidatorResolver>();

            builder.RegisterAssemblyTypes(catalog.Assemblies).AsClosedTypesOf(typeof(IValidator<>)).SingleInstance();
        }
    }
}
