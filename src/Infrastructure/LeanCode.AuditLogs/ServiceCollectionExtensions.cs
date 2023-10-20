using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.AuditLogs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuditLogs<TAuditLogStorage>(this IServiceCollection services)
        where TAuditLogStorage : class, IAuditLogStorage
    {
        services.AddTransient<AuditLogsPublisher>();
        services.AddTransient<IAuditLogStorage, TAuditLogStorage>();
        return services;
    }

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
