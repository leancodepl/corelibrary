using Autofac;
using LeanCode.Components;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace LeanCode.CQRS.MvcValidation
{
    class MvcValidationModule : Autofac.Module
    {
        private readonly TypesCatalog catalog;

        public MvcValidationModule(TypesCatalog catalog)
        {
            this.catalog = catalog;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(_ => new ActionContextAccessor()).As<IActionContextAccessor>().SingleInstance();
            builder.RegisterType<AutofacCommandResultTranslatorResolver>().As<ICommandResultTranslatorResolver>();
            builder.RegisterType<ValidatedCommandExecutor>().AsSelf();
            builder.RegisterType<ValidatedRemoteCommandExecutor>().AsSelf();

            builder.RegisterAssemblyTypes(catalog.Assemblies).AsClosedTypesOf(typeof(ICommandResultTranslator<>)).SingleInstance();
        }
    }
}
