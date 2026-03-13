using FluentAssertions;
using LeanCode.ServiceToService.Incoming;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace LeanCode.ServiceToService.Tests.Incoming;

public class ServiceToServiceAuthenticationExtensionsTests
{
    [Fact]
    public void Policy_scheme_routes_to_default_for_missing_s2s_api_key_header()
    {
        var selected = SelectScheme(headers: [], defaultScheme: "Kratos");

        selected.Should().Be("Kratos");
    }

    [Fact]
    public void Policy_scheme_routes_to_service_to_service_for_present_s2s_api_key_header()
    {
        var selected = SelectScheme(
            headers: new() { [ServiceToServiceDefaults.S2SApiKeyHeaderName] = "some-api-key" },
            defaultScheme: "Kratos"
        );

        selected.Should().Be(ServiceToServiceDefaults.AuthenticationScheme);
    }

    private static string SelectScheme(Dictionary<string, StringValues> headers, string defaultScheme)
    {
        using var provider = new ServiceCollection()
            .AddAuthentication()
            .AddServiceToServicePolicyScheme(defaultScheme: defaultScheme)
            .Services.BuildServiceProvider();

        var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<PolicySchemeOptions>>();
        var options = optionsMonitor.Get(ServiceToServiceDefaults.PolicyScheme);
        var context = new DefaultHttpContext();
        foreach (var (key, value) in headers)
        {
            context.Request.Headers[key] = value;
        }

        return options.ForwardDefaultSelector?.Invoke(context)
            ?? throw new InvalidOperationException("Policy scheme selector should always resolve a target scheme.");
    }
}
