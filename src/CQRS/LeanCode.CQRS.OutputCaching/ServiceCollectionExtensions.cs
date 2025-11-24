using LeanCode.Components;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.OutputCaching.Registration;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LeanCode.CQRS.OutputCaching;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCQRSOutputCache(
        this IServiceCollection services,
        TypesCatalog catalog,
        Action<OutputCacheOptions>? configureOptions = null
    )
    {
        if (configureOptions is null)
        {
            services.AddOutputCache();
        }
        else
        {
            services.AddOutputCache(configureOptions);
        }

        var registry = new CQRSOutputCacheRegistry();

        foreach (var policy in CQRSOutputCacheRegistry.EnumeratePolicies(catalog.Assemblies))
        {
            registry.Register(policy.ContractType, policy.PolicyType);
            services.AddTransient(policy.PolicyType);
        }

        services.AddSingleton(registry);
        services.AddSingleton<ICQRSEndpointMetadataProvider>(registry);
        services.AddSingleton<IConfigureOptions<OutputCacheOptions>>(sp => new CQRSOutputCacheOptionsConfigurator(
            sp.GetRequiredService<CQRSOutputCacheRegistry>()
        ));

        return services;
    }
}
