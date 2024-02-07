using FluentAssertions;
using LeanCode.CQRS.AspNetCore.Local.Context;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Local.Context;

public class NullEndpointFeatureTests
{
    [Fact]
    public void Endpoint_always_returns_null()
    {
        NullEndpointFeature.Empty.Endpoint.Should().BeNull();
        NullEndpointFeature.Empty.Endpoint = new(null, null, null);
        NullEndpointFeature.Empty.Endpoint.Should().BeNull();
    }
}
