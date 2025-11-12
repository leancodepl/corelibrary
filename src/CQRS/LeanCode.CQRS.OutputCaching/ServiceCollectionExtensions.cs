using System.Reflection;
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
        bool registerOutputCache = true)
    {
        return services.AddCQRSOutputCache(catalog.Assemblies, registerOutputCache);
    }

    public static IServiceCollection AddCQRSOutputCache(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies,
        bool registerOutputCache = true)
    {
        if (registerOutputCache)
        {
            services.AddOutputCache();
        }

        var registry = new CQRSOutputCacheRegistry();

        foreach (var policy in CQRSOutputCacheRegistry.EnumeratePolicies(assemblies))
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
