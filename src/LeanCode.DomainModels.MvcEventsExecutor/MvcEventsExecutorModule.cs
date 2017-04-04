using Autofac;

namespace LeanCode.DomainModels.MvcEventsExecutor
{
    class MvcEventsExecutorModule : Module
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
