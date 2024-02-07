using System.Net;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using LeanCode.CQRS.AspNetCore.Local.Context;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Local.Context;

public class NullConnectionInfoTests
{
    private readonly NullConnectionInfo connectionInfo = NullConnectionInfo.Empty;

    [Fact]
    public void Id_is_always_empty()
    {
        connectionInfo.Id.Should().BeEmpty();
        connectionInfo.Id = "other";
        connectionInfo.Id.Should().BeEmpty();
    }

    [Fact]
    public void RemoteIpAddress_always_returns_null()
    {
        connectionInfo.RemoteIpAddress.Should().BeNull();
        connectionInfo.RemoteIpAddress = new(0x2414188f);
        connectionInfo.RemoteIpAddress.Should().BeNull();
    }

    [Fact]
    public void RemotePort_always_returns_zero()
    {
        connectionInfo.RemotePort.Should().Be(0);
        connectionInfo.RemotePort = 123;
        connectionInfo.RemotePort.Should().Be(0);
    }

    [Fact]
    public void LocalIpAddress_always_returns_null()
    {
        connectionInfo.LocalIpAddress.Should().BeNull();
        connectionInfo.LocalIpAddress = new(0x2414188f);
        connectionInfo.LocalIpAddress.Should().BeNull();
    }

    [Fact]
    public void LocalPort_always_returns_zero()
    {
        connectionInfo.LocalPort.Should().Be(0);
        connectionInfo.LocalPort = 123;
        connectionInfo.LocalPort.Should().Be(0);
    }

    [Fact]
    public void ClientCertificate_always_returns_null()
    {
        connectionInfo.ClientCertificate.Should().BeNull();
        connectionInfo.ClientCertificate = Substitute.For<X509Certificate2>();
        connectionInfo.ClientCertificate.Should().BeNull();
    }

    [Fact]
    public async Task GetClientCertificateAsync_always_returns_null()
    {
        var result = await connectionInfo.GetClientCertificateAsync();
        result.Should().BeNull();
    }
}
