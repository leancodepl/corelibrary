using LeanCode.AuditLogs;

namespace Microsoft.Extensions.DependencyInjection;

public static class AuditLogsExtensions
{
    public static IServiceCollection AddAzureStorageAuditLogs(
        this IServiceCollection services,
        AzureBlobAuditLogStorageConfiguration config
    )
    {
        services.AddSingleton(config);
        services.AddTransient<AuditLogsPublisher>();
        services.AddTransient<IAuditLogStorage, AzureBlobAuditLogStorage>();
        return services;
    }
}
