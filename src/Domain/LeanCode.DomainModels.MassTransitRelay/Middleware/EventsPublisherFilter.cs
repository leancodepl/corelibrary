using System.Linq;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Registration;

namespace LeanCode.DomainModels.MassTransitRelay.Middleware
{
    public class EventsPublisherFilter<TConsumer, TMessage> : IFilter<ConsumerConsumeContext<TConsumer, TMessage>>
        where TConsumer : class
        where TMessage : class
    {
        private readonly AsyncEventsInterceptor interceptor;

        public EventsPublisherFilter(AsyncEventsInterceptor interceptor)
        {
            this.interceptor = interceptor;
        }

        public void Probe(ProbeContext context) { }

        public async Task Send(ConsumerConsumeContext<TConsumer, TMessage> context, IPipe<ConsumerConsumeContext<TConsumer, TMessage>> next)
        {
            var raisedEvents = await interceptor.CaptureEventsOfAsync(() => next.Send(context));

            var publishTasks = raisedEvents.Select(evt => context.Publish((object)evt, cfg => cfg.MessageId = evt.Id));
            await Task.WhenAll(publishTasks);
        }
    }

    public static class EventsPublisherFilterExtensions
    {
        public static void UseDomainEventsPublishing(
            this IConsumePipeConfigurator configurator,
            IConfigurationServiceProvider provider)
        {
            configurator.UseTypedConsumeFilter<Observer>(provider);
        }

        private class Observer : ScopedTypedConsumerConsumePipeSpecificationObserver
        {
            public override void ConsumerMessageConfigured<TConsumer, TMessage>(IConsumerMessageConfigurator<TConsumer, TMessage> configurator) =>
                configurator.AddConsumerScopedFilter<EventsPublisherFilter<TConsumer, TMessage>, TConsumer, TMessage>(Provider);
        }
    }
}
