using FluentAssertions;
using LeanCode.CQRS.AspNetCore.Local.Context;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Local.Context;

public class NullEndpointFeatureTests
{
    [Fact]
    public void Endpoint_always_returns_null()
    {
        NullEndpointFeature.Empty.Endpoint.Should().BeNull();
        NullEndpointFeature.Empty.Endpoint = Substitute.For<Endpoint>();
        NullEndpointFeature.Empty.Endpoint.Should().BeNull();
    }
}
