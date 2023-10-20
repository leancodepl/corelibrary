using MassTransit;

namespace LeanCode.AuditLogs;

public static class MassTransitRegistrationConfigurationExtensions
{
    public static void AddAuditLogsConsumer(this IRegistrationConfigurator configurator)
    {
        configurator.AddConsumer(typeof(AuditLogsConsumer));
    }
}
