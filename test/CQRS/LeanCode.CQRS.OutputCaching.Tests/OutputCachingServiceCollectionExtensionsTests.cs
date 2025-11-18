using FluentAssertions;
using LeanCode.Components;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.OutputCaching.Registration;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace LeanCode.CQRS.OutputCaching.Tests;

public class OutputCachingServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCQRSOutputCache_registers_registry_and_policies()
    {
        var services = new ServiceCollection();

        services.AddCQRSOutputCache(TypesCatalog.Of<TestQueryOCP>());

        services
            .Should()
            .ContainSingle(descriptor =>
                descriptor.ServiceType == typeof(TestQueryOCP) && descriptor.Lifetime == ServiceLifetime.Transient
            );

        using var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<CQRSOutputCacheRegistry>();
        registry.PolicyForObject.Should().ContainKey(typeof(TestQuery)).WhoseValue.Should().Be<TestQueryOCP>();

        provider.GetRequiredService<ICQRSEndpointMetadataProvider>().Should().BeSameAs(registry);

        provider
            .GetServices<IConfigureOptions<OutputCacheOptions>>()
            .OfType<CQRSOutputCacheOptionsConfigurator>()
            .Should()
            .ContainSingle();
    }
}
