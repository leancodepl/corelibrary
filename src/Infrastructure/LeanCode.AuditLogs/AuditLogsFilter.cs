using System.Diagnostics;
using LeanCode.CQRS.MassTransitRelay;
using LeanCode.OpenTelemetry;
using LeanCode.TimeProvider;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.AuditLogs;

public class AuditLogsFilter<TDbContext, TConsumer, TMessage> : IFilter<ConsumerConsumeContext<TConsumer, TMessage>>
    where TDbContext : DbContext
    where TConsumer : class
    where TMessage : class
{
    private readonly TDbContext dbContext;
    private readonly IBus bus;

    public AuditLogsFilter(TDbContext dbContext, IBus bus)
    {
        this.dbContext = dbContext;
        this.bus = bus;
    }

    public void Probe(ProbeContext context) { }

    public async Task Send(
        ConsumerConsumeContext<TConsumer, TMessage> context,
        IPipe<ConsumerConsumeContext<TConsumer, TMessage>> next
    )
    {
        await next.Send(context);

        var entitiesChanged = ChangedEntitiesExtractor.Extract(dbContext);
        if (entitiesChanged.Count != 0)
        {
            var actorId = Activity.Current?.GetBaggageItem(IdentityTraceBaggageHelpers.CurrentUserIdKey);
            var actionName = context.Consumer.ToString()!;
            var now = Time.NowWithOffset;

            await Task.WhenAll(
                entitiesChanged.Select(
                    e =>
                        bus.Publish(
                            new AuditLogMessage(
                                e,
                                actionName,
                                now,
                                actorId,
                                Activity.Current?.TraceId.ToString(),
                                Activity.Current?.SpanId.ToString()
                            ),
                            context.CancellationToken
                        )
                )
            );
        }
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
