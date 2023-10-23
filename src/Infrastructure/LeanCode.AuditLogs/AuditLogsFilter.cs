using LeanCode.CQRS.MassTransitRelay;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.AuditLogs;

public class AuditLogsFilter<TDbContext, TConsumer, TMessage> : IFilter<ConsumerConsumeContext<TConsumer, TMessage>>
    where TDbContext : DbContext
    where TConsumer : class
    where TMessage : class
{
    private readonly TDbContext dbContext;
    private readonly AuditLogsPublisher auditLogsPublisher;

    public AuditLogsFilter(TDbContext dbContext, AuditLogsPublisher auditLogsPublisher)
    {
        this.dbContext = dbContext;
        this.auditLogsPublisher = auditLogsPublisher;
    }

    public void Probe(ProbeContext context) { }

    public async Task Send(
        ConsumerConsumeContext<TConsumer, TMessage> context,
        IPipe<ConsumerConsumeContext<TConsumer, TMessage>> next
    )
    {
        await next.Send(context);
        await auditLogsPublisher.ExtractAndPublishAsync(
            dbContext,
            context,
            context.Consumer.ToString()!,
            context.CancellationToken
        );
    }
}

public static class EventsPublisherFilterExtensions
{
    public static void UseAuditLogs<TDbContext>(this IConsumePipeConfigurator configurator, IServiceProvider provider)
        where TDbContext : DbContext
    {
        configurator.UseTypedConsumeFilter<Observer<TDbContext>>(provider);
    }

    private sealed class Observer<TDbContext> : ScopedTypedConsumerConsumePipeSpecificationObserver
        where TDbContext : DbContext
    {
        public override void ConsumerMessageConfigured<TConsumer, TMessage>(
            IConsumerMessageConfigurator<TConsumer, TMessage> configurator
        ) =>
            configurator.AddConsumerScopedFilter<AuditLogsFilter<TDbContext, TConsumer, TMessage>, TConsumer, TMessage>(
                Provider
            );
    }
}
