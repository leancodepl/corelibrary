using System.Linq;
using System.Threading.Tasks;
using GreenPipes;
using LeanCode.DomainModels.EventsExecution;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.PipeConfigurators;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public class EventsPublisherFilter<TConsumer, TMessage> : IFilter<ConsumerConsumeContext<TConsumer, TMessage>>
        where TConsumer : class
        where TMessage : class
    {
        private readonly IPipeContextServiceResolver serviceResolver;

        public EventsPublisherFilter(IPipeContextServiceResolver serviceResolver)
        {
            this.serviceResolver = serviceResolver;
        }

        public void Probe(ProbeContext context) { }

        public async Task Send(ConsumerConsumeContext<TConsumer, TMessage> context, IPipe<ConsumerConsumeContext<TConsumer, TMessage>> next)
        {
            var interceptor = serviceResolver.GetService<AsyncEventsInterceptor>(context);
            interceptor.Prepare();

            await next.Send(context);

            var queue = interceptor.CaptureQueue();
            var publishTasks = queue.Select(evt => context.Publish((object)evt, cfg => cfg.MessageId = evt.Id));
            await Task.WhenAll(publishTasks);
        }
    }

    public static class EventsPublisherFilterExtensions
    {
        public static void UseDomainEventsPublishing(
            this IConsumePipeConfigurator config,
            IPipeContextServiceResolver? serviceResolver = null)
        {
            serviceResolver ??= AutofacPipeContextServiceResolver.Instance;
            _ = new EventsPublisherFilterConfigurationObserver(config, serviceResolver);
        }
    }

    public class EventsPublisherFilterConfigurationObserver :
        ConfigurationObserver,
        IConsumerConfigurationObserver
    {
        private readonly IPipeContextServiceResolver serviceResolver;

        public EventsPublisherFilterConfigurationObserver(
            IConsumePipeConfigurator configurator,
            IPipeContextServiceResolver serviceResolver)
            : base(configurator)
        {
            this.serviceResolver = serviceResolver;
        }

        public void ConsumerMessageConfigured<TConsumer, TMessage>(IConsumerMessageConfigurator<TConsumer, TMessage> configurator)
            where TConsumer : class
            where TMessage : class
        {
            configurator.UseFilter(new EventsPublisherFilter<TConsumer, TMessage>(serviceResolver));
        }
    }
}
