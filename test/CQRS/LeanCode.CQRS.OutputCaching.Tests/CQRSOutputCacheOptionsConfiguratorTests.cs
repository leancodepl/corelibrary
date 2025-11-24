using FluentAssertions;
using LeanCode.CQRS.OutputCaching.Registration;
using Microsoft.AspNetCore.OutputCaching;
using Xunit;

namespace LeanCode.CQRS.OutputCaching.Tests;

public class CQRSOutputCacheOptionsConfiguratorTests
{
    [Fact]
    public void Configure_registers_named_output_cache_policies_for_all_registry_entries()
    {
        var registry = new CQRSOutputCacheRegistry();
        registry.Register(typeof(TestQuery), typeof(TestQueryOCP));
        registry.Register(typeof(TestOperation), typeof(TestOperationOCP));

        var options = new OutputCacheOptions();
        var configurator = new RecordingConfigurator(registry);

        configurator.Configure(options);

        configurator
            .RegisteredPolicies.Should()
            .BeEquivalentTo(
                [
                    (CQRSOutputCachePolicyName.For(typeof(TestQueryOCP)), typeof(TestQueryOCP)),
                    (CQRSOutputCachePolicyName.For(typeof(TestOperationOCP)), typeof(TestOperationOCP)),
                ]
            );
    }

    private sealed class RecordingConfigurator(CQRSOutputCacheRegistry registry)
        : CQRSOutputCacheOptionsConfigurator(registry)
    {
        public readonly List<(string PolicyName, Type PolicyType)> RegisteredPolicies = [];

        protected override void RegisterPolicy(OutputCacheOptions options, string policyName, Type policyType)
        {
            RegisteredPolicies.Add((policyName, policyType));
            base.RegisterPolicy(options, policyName, policyType);
        }
    }
}
