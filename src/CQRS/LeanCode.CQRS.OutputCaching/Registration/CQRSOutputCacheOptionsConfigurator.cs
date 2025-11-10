using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Options;

namespace LeanCode.CQRS.OutputCaching.Registration;

public sealed class CQRSOutputCacheOptionsConfigurator : IConfigureOptions<OutputCacheOptions>
{
    private readonly CQRSOutputCacheRegistry registry;

    public CQRSOutputCacheOptionsConfigurator(CQRSOutputCacheRegistry registry)
    {
        this.registry = registry;
    }

    public void Configure(OutputCacheOptions options)
    {
        foreach (var descriptor in registry.PolicyForObject.Values)
        {
            var name = CQRSOutputCachePolicyName.For(descriptor.PolicyType);
            options.AddPolicy(name, builder => builder.AddPolicy(descriptor.PolicyType), true);
        }
    }
}
