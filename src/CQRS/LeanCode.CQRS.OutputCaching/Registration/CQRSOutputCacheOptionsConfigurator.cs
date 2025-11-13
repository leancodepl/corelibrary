using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Options;

namespace LeanCode.CQRS.OutputCaching.Registration;

internal class CQRSOutputCacheOptionsConfigurator : IConfigureOptions<OutputCacheOptions>
{
    private readonly CQRSOutputCacheRegistry registry;

    public CQRSOutputCacheOptionsConfigurator(CQRSOutputCacheRegistry registry)
    {
        this.registry = registry;
    }

    public void Configure(OutputCacheOptions options)
    {
        foreach (var policyType in registry.PolicyForObject.Values)
        {
            var name = CQRSOutputCachePolicyName.For(policyType);
            RegisterPolicy(options, name, policyType);
        }
    }

    protected virtual void RegisterPolicy(OutputCacheOptions options, string policyName, Type policyType)
    {
        options.AddPolicy(policyName, builder => builder.AddPolicy(policyType), true);
    }
}
