using System.Linq;
using System.Threading.Tasks;
using GreenPipes;
using LeanCode.DomainModels.MassTransitRelay.Inbox;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.PipeConfigurators;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public class ConsumeMessageOnceFilter<TConsumer, TMessage> : IFilter<ConsumerConsumeContext<TConsumer, TMessage>>
        where TConsumer : class
        where TMessage : class
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<ConsumeMessageOnceFilter<TConsumer, TMessage>>();
        private readonly IPipeContextServiceResolver serviceResolver;

        public ConsumeMessageOnceFilter(IPipeContextServiceResolver serviceResolver)
        {
            this.serviceResolver = serviceResolver;
        }

        public void Probe(ProbeContext context)
        { }

        public async Task Send(ConsumerConsumeContext<TConsumer, TMessage> context, IPipe<ConsumerConsumeContext<TConsumer, TMessage>> next)
        {
            var consumedMessages = serviceResolver.GetService<IConsumedMessagesContext>(context);

            var msg = ConsumedMessage.Create(context);

            if (await consumedMessages.ConsumedMessages
                .Where(m => m.ConsumerType == msg.ConsumerType && m.MessageId == msg.MessageId)
                .AnyAsync())
            {
                logger.Information("Message {MessageId} already consumed by {ConsumerType}", msg.MessageId, msg.ConsumerType);
                return;
            }
            else
            {
                consumedMessages.ConsumedMessages.Add(msg);
                await next.Send(context);
                await consumedMessages.SaveChangesAsync();
            }
        }
    }

    public static class ConsumeMessageOnceFilterExtensions
    {
        public static void UseConsumedMessagesFiltering(
            this IConsumePipeConfigurator configurator,
            IPipeContextServiceResolver? serviceResolver = null)
        {
            serviceResolver ??= AutofacPipeContextServiceResolver.Instance;
            _ = new ConsumeMessageOnceFilterConfigurationObserver(configurator, serviceResolver);
        }
    }

    public class ConsumeMessageOnceFilterConfigurationObserver :
       ConfigurationObserver,
       IConsumerConfigurationObserver
    {
        private readonly IPipeContextServiceResolver serviceResolver;

        public ConsumeMessageOnceFilterConfigurationObserver(
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
            configurator.UseFilter(new ConsumeMessageOnceFilter<TConsumer, TMessage>(serviceResolver));
        }
    }
}
