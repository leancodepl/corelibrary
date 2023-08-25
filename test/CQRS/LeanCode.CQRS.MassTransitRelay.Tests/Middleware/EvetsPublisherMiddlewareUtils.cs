using LeanCode.Contracts;
using Leancode.CQRS.MassTransitRelay;
using LeanCode.CQRS.MassTransitRelay.Middleware;
using LeanCode.CQRS.MassTransitRelay.Tests.Integration;
using LeanCode.DomainModels.Model;
using MassTransit;

namespace LeanCode.CQRS.MassTransitRelay.Tests.Middleware;

public class TestCommand : ICommand { }

public class IgnoreType { }

public class TestEvent1 : IDomainEvent
{
    public Guid Id { get; private set; }
    public DateTime DateOccurred { get; private set; }

    public TestEvent1()
    {
        Id = Guid.NewGuid();
        DateOccurred = DateTime.Now;
    }
}

public class TestEvent2 : IDomainEvent
{
    public Guid Id { get; private set; }
    public DateTime DateOccurred { get; private set; }

    public TestEvent2()
    {
        Id = Guid.NewGuid();
        DateOccurred = DateTime.Now;
    }
}

public class TestEventConsumer : IConsumer<TestEvent1>
{
    public Task Consume(ConsumeContext<TestEvent1> context)
    {
        DomainEvents.Raise(new TestEvent2());
        return Task.CompletedTask;
    }
}

public sealed class TestEventConsumerDefinition : ConsumerDefinition<TestEventConsumer>
{
    private readonly IServiceProvider serviceProvider;

    public TestEventConsumerDefinition(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<TestEventConsumer> consumerConfigurator
    )
    {
        endpointConfigurator.UseDomainEventsPublishing(serviceProvider);
        endpointConfigurator.ConfigureSend(cfg => ConsumerConfigurationExtensions.ConfigureSendActorIdPropagation(cfg));
        endpointConfigurator.ConfigurePublish(
            cfg => ConsumerConfigurationExtensions.ConfigurePublishActorIdPropagation(cfg)
        );
    }
}
