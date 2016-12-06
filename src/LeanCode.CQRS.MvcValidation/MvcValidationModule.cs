using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace LeanCode.CQRS.MvcValidation
{
    public class MvcValidationModule : Autofac.Module
    {
        private readonly Assembly[] assemblies;

        public MvcValidationModule(Type[] searchAssemblies)
        {
            assemblies = searchAssemblies
                .Select(t => t.GetTypeInfo().Assembly)
                .ToArray();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(_ => new ActionContextAccessor()).As<IActionContextAccessor>().SingleInstance();
            builder.RegisterType<AutofacCommandResultTranslatorResolver>().As<ICommandResultTranslatorResolver>();
            builder.RegisterType<ValidatedCommandExecutor>().AsSelf();
            builder.RegisterType<ValidatedRemoteCommandExecutor>().AsSelf();

            builder.RegisterAssemblyTypes(assemblies).AsClosedTypesOf(typeof(ICommandResultTranslator<>)).SingleInstance();
        }
    }
}
