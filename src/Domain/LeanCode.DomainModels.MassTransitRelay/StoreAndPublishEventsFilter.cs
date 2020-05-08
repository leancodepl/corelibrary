using System.Linq;
using System.Threading.Tasks;
using GreenPipes;
using LeanCode.DomainModels.EventsExecution;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.PipeConfigurators;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public class StoreAndPublishEventsFilter<TConsumer, TMessage> : IFilter<ConsumerConsumeContext<TConsumer, TMessage>>
        where TConsumer : class
        where TMessage : class
    {
        private readonly IPipeContextServiceResolver serviceResolver;

        public StoreAndPublishEventsFilter(IPipeContextServiceResolver serviceResolver)
        {
            this.serviceResolver = serviceResolver;
        }

        public void Probe(ProbeContext context) { }

        public async Task Send(ConsumerConsumeContext<TConsumer, TMessage> context, IPipe<ConsumerConsumeContext<TConsumer, TMessage>> next)
        {
            var interceptor = serviceResolver.GetService<AsyncEventsInterceptor>(context);
            var impl = serviceResolver.GetService<StoreAndPublishEventsImpl>(context);
            interceptor.Prepare();

            await next.Send(context);

            var queue = interceptor.CaptureQueue();
            await impl.StoreAndPublishEvents(queue.ToList(), context.ConversationId!.Value, new EventPublisher(context));
        }
    }

    public static class StoreAndPublishEventsFilterExtensions
    {
        public static void StoreAndPublishDomainEvents(
            this IConsumePipeConfigurator config,
            IPipeContextServiceResolver? serviceResolver = null)
        {
            serviceResolver ??= AutofacPipeContextServiceResolver.Instance;
            _ = new StoreAndPublishEventsFilterConfigurationObserver(config, serviceResolver);
        }
    }

    public class StoreAndPublishEventsFilterConfigurationObserver :
        ConfigurationObserver,
        IConsumerConfigurationObserver
    {
        private readonly IPipeContextServiceResolver serviceResolver;

        public StoreAndPublishEventsFilterConfigurationObserver(
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
            configurator.UseFilter(new StoreAndPublishEventsFilter<TConsumer, TMessage>(serviceResolver));
        }
    }
}
