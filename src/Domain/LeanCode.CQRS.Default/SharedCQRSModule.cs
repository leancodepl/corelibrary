using Autofac;
using Autofac.Features.Variance;
using LeanCode.CQRS.Cache;
using LeanCode.CQRS.Default.Autofac;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Validation;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.DomainModels.EventsExecution.Simple;
using LeanCode.Pipelines;
using LeanCode.Pipelines.Autofac;

namespace LeanCode.CQRS.Default
{
    internal class SharedCQRSModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AutofacPipelineFactory>()
                .As<IPipelineFactory>()
                .SingleInstance();

            builder.RegisterGeneric(typeof(CQRSSecurityElement<,,>)).AsSelf();
            builder.RegisterGeneric(typeof(ValidationElement<>)).AsSelf();
            builder.RegisterGeneric(typeof(CacheElement<>)).AsSelf();
            builder.RegisterGeneric(typeof(CommandFinalizer<>)).AsSelf();
            builder.RegisterGeneric(typeof(QueryFinalizer<>)).AsSelf();
            builder.RegisterGeneric(typeof(EventsInterceptorElement<,,>)).AsSelf();
            builder.RegisterGeneric(typeof(EventsExecutorElement<,,>)).AsSelf();

            builder.RegisterType<RoleRegistry>().AsSelf().SingleInstance();
            builder.RegisterType<DefaultPermissionAuthorizer>().AsSelf().AsImplementedInterfaces();

            builder.RegisterType<AsyncEventsInterceptor>()
                .AsSelf()
                .OnActivated(a => a.Instance.Configure())
                .SingleInstance();

            builder.RegisterType<RetryPolicies>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<SimpleEventsExecutor>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<SimpleFinalizer>().AsSelf();

            builder.RegisterType<AutofacEventHandlerResolver>().As<IDomainEventHandlerResolver>();

            builder.RegisterSource(new ContravariantRegistrationSource());
        }
    }
}
