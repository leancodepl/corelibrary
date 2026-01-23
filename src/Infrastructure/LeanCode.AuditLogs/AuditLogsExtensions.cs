using System.Text.Json;
using LeanCode.AuditLogs;

namespace Microsoft.Extensions.DependencyInjection;

public static class AuditLogsExtensions
{
    public static IServiceCollection AddAzureStorageAuditLogs(
        this IServiceCollection services,
        AzureBlobAuditLogStorageConfiguration config,
        JsonSerializerOptions? jsonSerializerOptions = null
    )
    {
        services.AddSingleton(config);
        if (jsonSerializerOptions is not null)
        {
            services.AddSingleton(jsonSerializerOptions);
        }
        services.AddTransient<AuditLogsPublisher>();
        services.AddTransient<IAuditLogStorage, AzureBlobAuditLogStorage>();
        return services;
    }
}
