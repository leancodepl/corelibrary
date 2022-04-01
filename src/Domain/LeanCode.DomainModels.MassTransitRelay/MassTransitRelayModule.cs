using Autofac;
using LeanCode.Components;
using LeanCode.DomainModels.MassTransitRelay.Inbox;
using LeanCode.DomainModels.MassTransitRelay.Middleware;
using LeanCode.DomainModels.MassTransitRelay.Outbox;
using LeanCode.DomainModels.MassTransitRelay.Simple;
using LeanCode.PeriodicService;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public abstract class MassTransitRelayModule : AppModule
    {
        private readonly TypesCatalog eventsCatalog;
        private readonly bool useInbox;
        private readonly bool useOutbox;

        protected MassTransitRelayModule(
            TypesCatalog eventsCatalog,
            bool useInbox = true,
            bool useOutbox = true)
        {
            this.eventsCatalog = eventsCatalog;
            this.useInbox = useInbox;
            this.useOutbox = useOutbox;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(EventsPublisherElement<,,>)).AsSelf();
            builder.RegisterGeneric(typeof(StoreAndPublishEventsElement<,,>)).AsSelf();
            builder.RegisterType<EventPublisher>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<EventsStore>().AsSelf();

            if (useInbox)
            {
                builder.RegisterType<ConsumedMessagesCleaner>().AsSelf();
                builder.RegisterPeriodicAction<ConsumedMessagesCleaner>();
            }

            if (useOutbox)
            {
                builder.RegisterType<PeriodicEventsPublisher>().AsSelf();
                builder.RegisterType<PublishedEventsCleaner>().AsSelf();
                builder.RegisterPeriodicAction<PeriodicEventsPublisher>();
                builder.RegisterPeriodicAction<PublishedEventsCleaner>();
            }

            builder.RegisterInstance(new NewtonsoftJsonEventsSerializer(eventsCatalog))
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<AsyncEventsInterceptor>()
                .AsSelf()
                .OnActivated(a => a.Instance.Configure())
                .SingleInstance();

            builder.RegisterType<SimpleEventsExecutor>()
                .AsSelf()
                .SingleInstance()
                .WithParameter("useOutbox", useOutbox);

            builder.RegisterType<SimpleFinalizer>().AsSelf();
        }

        public sealed override void ConfigureServices(IServiceCollection services)
        {
            ConfigureMassTransit(services);
        }

        public abstract void ConfigureMassTransit(IServiceCollection services);
    }
}
