using MassTransit;
using MassTransit.Configuration;
using MassTransit.DependencyInjection;
using MassTransit.Middleware;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public static class TypedConsumerFilterExtensions
    {
        public static void UseTypedConsumeFilter<TObserver>(
            this IConsumePipeConfigurator configurator,
            IServiceProvider provider)
            where TObserver : ScopedTypedConsumerConsumePipeSpecificationObserver, new()
        {
            var observer = new TObserver { Provider = provider };
            configurator.ConnectConsumerConfigurationObserver(observer);
        }

        public static void AddConsumerScopedFilter<TFilter, TConsumer, TMessage>(
            this IPipeConfigurator<ConsumerConsumeContext<TConsumer, TMessage>> configurator,
            IServiceProvider provider)
            where TFilter : class, IFilter<ConsumerConsumeContext<TConsumer, TMessage>>
            where TConsumer : class
            where TMessage : class
        {
            var scopeProvider = new FilterScopeProvider<TFilter, ConsumerConsumeContext<TConsumer, TMessage>>(provider);
            var filter = new ScopedFilter<ConsumerConsumeContext<TConsumer, TMessage>>(scopeProvider);
            var specification = new FilterPipeSpecification<ConsumerConsumeContext<TConsumer, TMessage>>(filter);
            configurator.AddPipeSpecification(specification);
        }
    }

    public abstract class ScopedTypedConsumerConsumePipeSpecificationObserver : IConsumerConfigurationObserver
    {
        public IServiceProvider Provider { get; internal set; } = default!;

        public void ConsumerConfigured<TConsumer>(IConsumerConfigurator<TConsumer> configurator)
            where TConsumer : class
        { }

        public abstract void ConsumerMessageConfigured<TConsumer, TMessage>(IConsumerMessageConfigurator<TConsumer, TMessage> configurator)
            where TConsumer : class
            where TMessage : class;
    }
}
