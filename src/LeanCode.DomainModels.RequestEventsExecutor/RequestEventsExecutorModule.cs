using Autofac;

namespace LeanCode.DomainModels.RequestEventsExecutor
{
    class RequestEventsExecutorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PerRequestEventsStorage>()
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<EventExecutor>().AsSelf().SingleInstance();
        }
    }
}
