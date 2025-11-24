using FluentAssertions;
using LeanCode.Components;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.CQRS.OutputCaching.Tests;

public class OutputCachingServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCQRSOutputCache_registers_proper_caching_policies_from_types_catalog()
    {
        var services = new ServiceCollection();

        services.AddCQRSOutputCache(TypesCatalog.Of<TestQueryOCP>());

        services
            .Where(d => d.ServiceType.IsAssignableTo(typeof(IOutputCachePolicy)))
            .Should()
            .OnlyContain(d => d.Lifetime == ServiceLifetime.Transient)
            .And.Satisfy(d => d.ServiceType == typeof(TestQueryOCP), d => d.ServiceType == typeof(TestOperationOCP));
    }
}
