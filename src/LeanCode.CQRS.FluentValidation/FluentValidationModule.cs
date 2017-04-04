using System;
using System.Linq;
using System.Reflection;
using Autofac;
using FluentValidation;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS.FluentValidation
{
    class FluentValidationModule : Autofac.Module
    {
        private readonly Assembly[] assemblies;

        public FluentValidationModule(Type[] searchAssemblies)
        {
            assemblies = searchAssemblies
                .Select(t => t.GetTypeInfo().Assembly)
                .ToArray();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AutofacFluentValidatorResolver>().As<ICommandValidatorResolver>();

            builder.RegisterAssemblyTypes(assemblies)
                .AsClosedTypesOf(typeof(IValidator<>))
                .SingleInstance();
        }
    }
}
