using MassTransit;

namespace LeanCode.AuditLogs;

public static class MassTransitRegistrationConfigurationExtensions
{
    public static void AddAuditLogsConsumer(this IRegistrationConfigurator configurator)
    {
        configurator.AddConsumer(typeof(AuditLogsConsumer), typeof(AuditLogsConsumerDefinition));
    }

    public static void AddAuditLogsConsumer<T>(this IRegistrationConfigurator configurator)
    {
        configurator.AddConsumer(typeof(AuditLogsConsumer), typeof(T));
    }
}

public class AuditLogsConsumerDefinition : ConsumerDefinition<AuditLogsConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<AuditLogsConsumer> consumerConfigurator,
        IRegistrationContext context
    )
    {
        endpointConfigurator.UseMessageRetry(r =>
            r.Immediate(1).Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5))
        );
    }
}
