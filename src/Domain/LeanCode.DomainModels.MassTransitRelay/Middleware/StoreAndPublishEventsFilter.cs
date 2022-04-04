using MassTransit;

namespace LeanCode.DomainModels.MassTransitRelay.Middleware
{
    public class StoreAndPublishEventsFilter<TConsumer, TMessage> : IFilter<ConsumerConsumeContext<TConsumer, TMessage>>
        where TConsumer : class
        where TMessage : class
    {
        private readonly AsyncEventsInterceptor interceptor;
        private readonly EventsStore store;

        public StoreAndPublishEventsFilter(AsyncEventsInterceptor interceptor, EventsStore store)
        {
            this.interceptor = interceptor;
            this.store = store;
        }

        public void Probe(ProbeContext context) { }

        public async Task Send(ConsumerConsumeContext<TConsumer, TMessage> context, IPipe<ConsumerConsumeContext<TConsumer, TMessage>> next)
        {
            var events = await interceptor.CaptureEventsOfAsync(() => next.Send(context));

            await store.StoreAndPublishEventsAsync(
                events,
                context.ConversationId,
                new EventPublisher(context),
                context.CancellationToken);
        }
    }

    public static class StoreAndPublishEventsFilterExtensions
    {
        public static void StoreAndPublishDomainEvents(
            this IConsumePipeConfigurator configurator,
            IServiceProvider provider)
        {
            configurator.UseTypedConsumeFilter<Observer>(provider);
        }

        private class Observer : ScopedTypedConsumerConsumePipeSpecificationObserver
        {
            public override void ConsumerMessageConfigured<TConsumer, TMessage>(IConsumerMessageConfigurator<TConsumer, TMessage> configurator) =>
                configurator.AddConsumerScopedFilter<StoreAndPublishEventsFilter<TConsumer, TMessage>, TConsumer, TMessage>(Provider);
        }
    }
}
