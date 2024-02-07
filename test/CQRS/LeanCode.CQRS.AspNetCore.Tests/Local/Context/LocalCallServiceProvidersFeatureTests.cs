using FluentAssertions;
using LeanCode.CQRS.AspNetCore.Local.Context;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Local.Context;

public class LocalCallServiceProvidersFeatureTests
{
    private readonly IServiceProvider serviceProvider = new ServiceCollection().BuildServiceProvider();

    [Fact]
    public void RequestServices_can_be_changed()
    {
        var feature = new LocalCallServiceProvidersFeature(serviceProvider);
        feature.RequestServices.Should().BeSameAs(serviceProvider);

        feature.RequestServices = new ServiceCollection().BuildServiceProvider();
        feature.RequestServices.Should().NotBeSameAs(serviceProvider);
    }
}
