using Autofac;
using GreenPipes;
using GreenPipes.Specifications;
using MassTransit;
using MassTransit.AutofacIntegration;
using MassTransit.AutofacIntegration.ScopeProviders;
using MassTransit.ConsumeConfigurators;
using MassTransit.Registration;
using MassTransit.Scoping.Filters;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public static class TypedConsumerFilterExtensions
    {
        public static void UseTypedConsumeFilter<TObserver>(
            this IConsumePipeConfigurator configurator,
            ILifetimeScopeProvider lifetimeScopeProvider)

            where TObserver : ScopedTypedConsumerConsumePipeSpecificationObserver, new()
        {
            var observer = new TObserver { Provider = lifetimeScopeProvider };
            configurator.ConnectConsumerConfigurationObserver(observer);
        }

        public static void UseTypedConsumeFilter<TObserver>(
            this IConsumePipeConfigurator configurator,
            IConfigurationServiceProvider provider)
            where TObserver : ScopedTypedConsumerConsumePipeSpecificationObserver, new()
        {
            var requiredService = provider.GetRequiredService<ILifetimeScope>();
            var lifetimeScopeProvider = new SingleLifetimeScopeProvider(requiredService);
            configurator.UseTypedConsumeFilter<TObserver>(lifetimeScopeProvider);
        }

        public static void AddConsumerScopedFilter<TFilter, TConsumer, TMessage>(
            this IPipeConfigurator<ConsumerConsumeContext<TConsumer, TMessage>> configurator,
            ILifetimeScopeProvider provider)
            where TFilter : class, IFilter<ConsumerConsumeContext<TConsumer, TMessage>>
            where TConsumer : class
            where TMessage : class
        {
            var scopeProvider = new AutofacFilterContextScopeProvider<TFilter, ConsumerConsumeContext<TConsumer, TMessage>, ConsumerConsumeContext<TConsumer, TMessage>>(provider);
            var filter = new ScopedFilter<ConsumerConsumeContext<TConsumer, TMessage>>(scopeProvider);
            var specification = new FilterPipeSpecification<ConsumerConsumeContext<TConsumer, TMessage>>(filter);
            configurator.AddPipeSpecification(specification);
        }
    }

    public abstract class ScopedTypedConsumerConsumePipeSpecificationObserver : IConsumerConfigurationObserver
    {
        public ILifetimeScopeProvider Provider { get; internal set; } = default!;

        public void ConsumerConfigured<TConsumer>(IConsumerConfigurator<TConsumer> configurator)
            where TConsumer : class
        { }

        public abstract void ConsumerMessageConfigured<TConsumer, TMessage>(IConsumerMessageConfigurator<TConsumer, TMessage> configurator)
            where TConsumer : class
            where TMessage : class;
    }
}
