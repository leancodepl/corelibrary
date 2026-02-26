using FluentAssertions;
using LeanCode.ServiceToService.Outgoing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.ServiceToService.Tests.Outgoing;

public class ServiceToServiceHttpClientExtensionsTests
{
    [Fact]
    public void Adds_caller_identity_header_to_configured_http_client()
    {
        const string ServiceName = "notifications-service";

        using var provider = new ServiceCollection()
            .AddHttpClient("outgoing")
            .AddCallerIdentity(ServiceName)
            .Services.BuildServiceProvider();

        var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var client = clientFactory.CreateClient("outgoing");

        client
            .DefaultRequestHeaders.Should()
            .ContainSingle(kv => kv.Key == ServiceToServiceDefaults.CallerIdHeaderName)
            .Which.Value.Should()
            .ContainSingle(ServiceName);
    }

    [Fact]
    public void Preserves_existing_http_client_configuration()
    {
        using var provider = new ServiceCollection()
            .AddHttpClient("outgoing")
            .ConfigureHttpClient(client => client.BaseAddress = new("https://example.com"))
            .AddCallerIdentity("admin-panel")
            .Services.BuildServiceProvider();

        var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var client = clientFactory.CreateClient("outgoing");

        client.BaseAddress.Should().Be(new Uri("https://example.com"));
    }
}
