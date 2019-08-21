using Autofac;
using FluentValidation;
using LeanCode.Components;

namespace LeanCode.CQRS.Validation.Fluent
{
    /// <summary>
    /// Module integrating command validation via FluentValidation library
    /// </summary>
    public class FluentValidationModule : AppModule
    {
        private readonly TypesCatalog catalog;

        /// <param name="catalog">
        /// Assemblies containing <see cref="IValidator"/> command validators to register in DI
        /// </param>
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
