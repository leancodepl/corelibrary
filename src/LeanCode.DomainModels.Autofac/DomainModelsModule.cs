using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Features.Variance;
using LeanCode.DomainModels.Model;
using Module = Autofac.Module;

namespace LeanCode.ComainModels.Autofac
{
    public class DomainModelsModule : Module
    {
        private readonly Assembly[] assemblies;

        public DomainModelsModule(Type[] searchAssemblies)
        {
            assemblies = searchAssemblies
                .Select(t => t.GetTypeInfo().Assembly)
                .ToArray();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterSource(new ContravariantRegistrationSource());

            builder.RegisterType<AutofacEventHandlerResolver>().As<IDomainEventHandlerResolver>();
            builder.RegisterAssemblyTypes(assemblies).AsClosedTypesOf(typeof(IDomainEventHandler<>));
        }
    }
}
