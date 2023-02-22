using Autofac;
using Autofac.Features.Variance;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Validation;
using LeanCode.Pipelines;
using LeanCode.Pipelines.Autofac;

namespace LeanCode.CQRS.Default;

internal class SharedCQRSModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AutofacPipelineFactory>()
            .As<IPipelineFactory>()
            .SingleInstance();

        builder.RegisterGeneric(typeof(CQRSSecurityElement<,,>)).AsSelf();
        builder.RegisterGeneric(typeof(ValidationElement<>)).AsSelf();
        builder.RegisterGeneric(typeof(CommandFinalizer<>)).AsSelf();
        builder.RegisterGeneric(typeof(QueryFinalizer<>)).AsSelf();
        builder.RegisterGeneric(typeof(OperationFinalizer<>)).AsSelf();

        builder.RegisterType<RoleRegistry>().AsSelf().SingleInstance();
        builder.RegisterType<DefaultPermissionAuthorizer>().AsSelf().AsImplementedInterfaces();

        builder.RegisterSource(new ContravariantRegistrationSource());
    }
}
