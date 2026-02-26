using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.ServiceToService.Tests;

public class ServiceToServiceHttpClientExtensionsTests
{
    [Fact]
    public void Adds_caller_identity_header_to_configured_http_client()
    {
        const string serviceName = "notifications-service";

        using var provider = new ServiceCollection()
            .AddHttpClient("outgoing")
            .AddCallerIdentity(serviceName)
            .Services.BuildServiceProvider();

        var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var client = clientFactory.CreateClient("outgoing");

        Assert.True(
            client.DefaultRequestHeaders.TryGetValues(ServiceToServiceConstants.CallerIdHeaderName, out var values)
        );
        Assert.Equal(new[] { serviceName }, values);
    }

    [Fact]
    public void Preserves_existing_http_client_configuration()
    {
        using var provider = new ServiceCollection()
            .AddHttpClient("outgoing")
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://example.com"))
            .AddCallerIdentity("admin-panel")
            .Services.BuildServiceProvider();

        var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var client = clientFactory.CreateClient("outgoing");

        Assert.Equal(new Uri("https://example.com"), client.BaseAddress);
    }
}
