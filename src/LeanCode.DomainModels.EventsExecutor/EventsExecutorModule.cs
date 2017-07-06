using Autofac;

namespace LeanCode.DomainModels.EventsExecutor
{
    class EventsExecutorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AsyncEventsStorage>()
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<RetryPolicies>().AsSelf().SingleInstance();
            builder.RegisterType<EventsExecutor>().AsSelf();
        }
    }
}
