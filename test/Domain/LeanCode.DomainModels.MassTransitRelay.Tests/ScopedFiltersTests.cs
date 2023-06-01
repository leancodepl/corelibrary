using System.Collections.Concurrent;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using LeanCode.Components;
using LeanCode.DomainModels.MassTransitRelay.Testing;
using MassTransit;
using MassTransit.Testing.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests;

public class ScopedFiltersTests : IAsyncLifetime, IDisposable
{
    private readonly IBusControl bus;
    private readonly IBusActivityMonitor activityMonitor;
    private readonly Interceptor interceptor = new();
    private readonly ServiceProvider serviceProvider;

    public ScopedFiltersTests()
    {
        var services = new ServiceCollection();

        services.AddMassTransit(cfg =>
        {
            cfg.AddConsumer(typeof(TestConsumer), typeof(TestConsumerDefinition));

            cfg.UsingInMemory(
                (ctx, cfg) =>
                {
                    cfg.ConfigureEndpoints(ctx);
                }
            );
        });
        services.AddBusActivityMonitor();
        services.AddScoped<TestService>();
        services.AddSingleton(interceptor);

        serviceProvider = services.BuildServiceProvider();
        bus = serviceProvider.GetRequiredService<IBusControl>();
        activityMonitor = serviceProvider.GetRequiredService<IBusActivityMonitor>();
    }

    [Fact]
    public async Task Consumer_and_filters_of_a_message_are_in_the_same_di_scope()
    {
        // The test is very side effects based.
        // We declare a service which should be created once per message consumption
        // and intercept if the filters and consumers have the same instance service injected.

        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        await bus.Publish(new Message(), ctx => ctx.MessageId = id1);
        await bus.Publish(new Message(), ctx => ctx.MessageId = id2);

        await activityMonitor.AwaitBusInactivity();

        var (f11, f12, c1) = (interceptor.GetFilter1(id1), interceptor.GetFilter2(id1), interceptor.GetConsumer(id1));

        var (f21, f22, c2) = (interceptor.GetFilter1(id2), interceptor.GetFilter2(id2), interceptor.GetConsumer(id2));

        Assert.NotNull(f11);
        Assert.Equal(f11, f12);
        Assert.Equal(f11, c1);

        Assert.NotNull(f21);
        Assert.Equal(f21, f22);
        Assert.Equal(f21, c2);

        Assert.NotEqual(f11, f21);
    }

    public Task InitializeAsync() => bus.StartAsync();

    public Task DisposeAsync() => bus.StopAsync();

    private sealed class TestService
    {
        public Guid InstanceId { get; }

        public TestService()
        {
            InstanceId = Guid.NewGuid();
        }
    }

    private sealed record Message;

    private sealed class Filter1<TConsumer, TMessage> : IFilter<ConsumerConsumeContext<TConsumer, TMessage>>
        where TConsumer : class
        where TMessage : class
    {
        private readonly TestService service;
        private readonly Interceptor interceptor;

        public Filter1(TestService service, Interceptor interceptor)
        {
            this.service = service;
            this.interceptor = interceptor;
        }

        public void Probe(ProbeContext context) { }

        public Task Send(
            ConsumerConsumeContext<TConsumer, TMessage> context,
            IPipe<ConsumerConsumeContext<TConsumer, TMessage>> next
        )
        {
            interceptor.SetFilter1(context.MessageId!.Value, service.InstanceId);
            return next.Send(context);
        }
    }

    private sealed class Filter1Observer : ScopedTypedConsumerConsumePipeSpecificationObserver
    {
        public static void UseFilter1(IConsumePipeConfigurator configurator, IServiceProvider provider)
        {
            configurator.UseTypedConsumeFilter<Filter1Observer>(provider);
        }

        public override void ConsumerMessageConfigured<TObserverConsumer, TObserverMessage>(
            IConsumerMessageConfigurator<TObserverConsumer, TObserverMessage> configurator
        ) =>
            configurator.AddConsumerScopedFilter<
                Filter1<TObserverConsumer, TObserverMessage>,
                TObserverConsumer,
                TObserverMessage
            >(Provider);
    }

    private sealed class Filter2<TConsumer, TMessage> : IFilter<ConsumerConsumeContext<TConsumer, TMessage>>
        where TConsumer : class
        where TMessage : class
    {
        private readonly TestService service;
        private readonly Interceptor interceptor;

        public Filter2(TestService service, Interceptor interceptor)
        {
            this.service = service;
            this.interceptor = interceptor;
        }

        public void Probe(ProbeContext context) { }

        public Task Send(
            ConsumerConsumeContext<TConsumer, TMessage> context,
            IPipe<ConsumerConsumeContext<TConsumer, TMessage>> next
        )
        {
            interceptor.SetFilter2(context.MessageId!.Value, service.InstanceId);
            return next.Send(context);
        }
    }

    private sealed class Filter2Observer : ScopedTypedConsumerConsumePipeSpecificationObserver
    {
        public static void UseFilter2(IConsumePipeConfigurator configurator, IServiceProvider provider)
        {
            configurator.UseTypedConsumeFilter<Filter2Observer>(provider);
        }

        public override void ConsumerMessageConfigured<TObserverConsumer, TObserverMessage>(
            IConsumerMessageConfigurator<TObserverConsumer, TObserverMessage> configurator
        ) =>
            configurator.AddConsumerScopedFilter<
                Filter2<TObserverConsumer, TObserverMessage>,
                TObserverConsumer,
                TObserverMessage
            >(Provider);
    }

    private sealed class TestConsumer : IConsumer<Message>
    {
        private readonly TestService service;
        private readonly Interceptor interceptor;

        public TestConsumer(TestService service, Interceptor interceptor)
        {
            this.service = service;
            this.interceptor = interceptor;
        }

        public Task Consume(ConsumeContext<Message> context)
        {
            interceptor.SetConsumer(context.MessageId!.Value, service.InstanceId);
            return Task.CompletedTask;
        }
    }

    private sealed class TestConsumerDefinition : ConsumerDefinition<TestConsumer>
    {
        private readonly IServiceProvider serviceProvider;

        public TestConsumerDefinition(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<TestConsumer> consumerConfigurator
        )
        {
            Filter1Observer.UseFilter1(endpointConfigurator, serviceProvider);
            Filter2Observer.UseFilter2(endpointConfigurator, serviceProvider);
        }
    }

    private sealed class Interceptor
    {
        public ConcurrentDictionary<string, Guid> Data { get; } = new();

        public void SetFilter1(Guid messageId, Guid value)
        {
            Data[$"{messageId}_filter1"] = value;
        }

        public void SetFilter2(Guid messageId, Guid value)
        {
            Data[$"{messageId}_filter2"] = value;
        }

        public void SetConsumer(Guid messageId, Guid value)
        {
            Data[$"{messageId}_consumer"] = value;
        }

        public Guid? GetFilter1(Guid messageId)
        {
            return Data.TryGetValue($"{messageId}_filter1", out var value) ? value : null;
        }

        public Guid? GetFilter2(Guid messageId)
        {
            return Data.TryGetValue($"{messageId}_filter2", out var value) ? value : null;
        }

        public Guid? GetConsumer(Guid messageId)
        {
            return Data.TryGetValue($"{messageId}_consumer", out var value) ? value : null;
        }
    }

    public void Dispose() => serviceProvider.Dispose();
}
