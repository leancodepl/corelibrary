using System.Collections.Concurrent;
using LeanCode.CQRS.MassTransitRelay.Middleware;
using LeanCode.DomainModels.Model;
using MassTransit;

namespace LeanCode.CQRS.MassTransitRelay.Tests.Integration;

public class Event1Consumer : IConsumer<Event1>
{
    private readonly TestDbContext dbContext;

    public Event1Consumer(TestDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task Consume(ConsumeContext<Event1> context)
    {
        var correlationId = context.Message.CorrelationId;

        DomainEvents.Raise(new Event2(correlationId));
        HandledLog.Report(dbContext, correlationId, nameof(Event1Consumer));
        return Task.CompletedTask;
    }
}

public class Event2FirstConsumer : IConsumer<Event2>
{
    private readonly TestDbContext dbContext;

    public Event2FirstConsumer(TestDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task Consume(ConsumeContext<Event2> context)
    {
        var correlationId = context.Message.CorrelationId;
        DomainEvents.Raise(new Event3(correlationId));
        HandledLog.Report(dbContext, correlationId, nameof(Event2FirstConsumer));
        return Task.CompletedTask;
    }
}

public class Event2SecondConsumer : IConsumer<Event2>
{
    private readonly TestDbContext dbContext;

    public Event2SecondConsumer(TestDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task Consume(ConsumeContext<Event2> context)
    {
        var correlationId = context.Message.CorrelationId;
        HandledLog.Report(dbContext, correlationId, nameof(Event2SecondConsumer));
        return Task.CompletedTask;
    }
}

public class Event2RetryingConsumer : IConsumer<Event2>
{
    private readonly TestDbContext dbContext;
    private static readonly ConcurrentDictionary<Guid, bool> IsRetry = new ConcurrentDictionary<Guid, bool>();

    public Event2RetryingConsumer(TestDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task Consume(ConsumeContext<Event2> context)
    {
        HandledLog.Report(dbContext, context.Message.CorrelationId, nameof(Event2RetryingConsumer));

        var isRetry = IsRetry.AddOrUpdate(context.Message.Id, false, (_, _) => true);
        if (!isRetry)
        {
            throw new InvalidOperationException("This handler fails for testing purposes");
        }

        return Task.CompletedTask;
    }
}

public class DefaultConsumerDefinition<TConsumer> : ConsumerDefinition<TConsumer>
    where TConsumer : class, IConsumer
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<TConsumer> consumerConfigurator,
        IRegistrationContext context
    )
    {
        endpointConfigurator.UseMessageRetry(r => r.Immediate(1));
        endpointConfigurator.UseEntityFrameworkOutbox<TestDbContext>(context);
        endpointConfigurator.UseDomainEventsPublishing(context);
    }
}
