using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.AuditLogs;

public static class ServiceCollectionExtensions
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
