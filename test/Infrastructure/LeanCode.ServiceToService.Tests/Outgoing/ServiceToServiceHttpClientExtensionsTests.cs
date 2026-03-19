using FluentAssertions;
using FluentAssertions.Execution;
using LeanCode.ServiceToService.Outgoing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.ServiceToService.Tests.Outgoing;

public class ServiceToServiceHttpClientExtensionsTests
{
    [Fact]
    public void Adds_caller_identity_and_s2s_api_key_headers_to_configured_http_client()
    {
        const string ServiceName = "notifications-service";
        const string S2SApiKey = "test-project-dev-api-key";

        using var provider = new ServiceCollection()
            .AddHttpClient("outgoing")
            .AddServiceToServiceCallerIdentity(ServiceName, S2SApiKey)
            .Services.BuildServiceProvider();

        var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var client = clientFactory.CreateClient("outgoing");

        using var _ = new AssertionScope();
        client
            .DefaultRequestHeaders.Should()
            .ContainSingle(kv => kv.Key == ServiceToServiceConsts.CallerIdHeaderName)
            .Which.Value.Should()
            .ContainSingle(ServiceName);
        client
            .DefaultRequestHeaders.Should()
            .ContainSingle(kv => kv.Key == ServiceToServiceConsts.S2SApiKeyHeaderName)
            .Which.Value.Should()
            .ContainSingle(S2SApiKey);
    }

    [Fact]
    public void Preserves_existing_http_client_configuration()
    {
        using var provider = new ServiceCollection()
            .AddHttpClient("outgoing")
            .ConfigureHttpClient(client => client.BaseAddress = new("https://example.com"))
            .AddServiceToServiceCallerIdentity("admin-panel", "test-project-dev-api-key")
            .Services.BuildServiceProvider();

        var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var client = clientFactory.CreateClient("outgoing");

        client.BaseAddress.Should().Be(new Uri("https://example.com"));
    }
}
