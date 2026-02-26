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
    public void Policy_scheme_routes_to_service_to_service_for_missing_header_by_default()
    {
        var selected = SelectScheme(
            headers: [],
            fallbackToDefaultSchemeOnMissingHeader: false,
            defaultScheme: "Kratos"
        );

        selected.Should().Be(ServiceToServiceDefaults.AuthenticationScheme);
    }

    [Fact]
    public void Policy_scheme_routes_to_default_for_missing_header_when_fallback_is_enabled()
    {
        var selected = SelectScheme(headers: [], fallbackToDefaultSchemeOnMissingHeader: true, defaultScheme: "Kratos");

        selected.Should().Be("Kratos");
    }

    [Fact]
    public void Policy_scheme_routes_to_default_for_ingress_caller()
    {
        var selected = SelectScheme(
            headers: new() { [ServiceToServiceDefaults.CallerIdHeaderName] = "traefik" },
            fallbackToDefaultSchemeOnMissingHeader: false,
            defaultScheme: "Kratos",
            ingressCallerId: "traefik"
        );

        selected.Should().Be("Kratos");
    }

    [Fact]
    public void Policy_scheme_routes_to_service_to_service_for_non_ingress_caller()
    {
        var selected = SelectScheme(
            headers: new() { [ServiceToServiceDefaults.CallerIdHeaderName] = "notifications-service" },
            fallbackToDefaultSchemeOnMissingHeader: false,
            defaultScheme: "Kratos",
            ingressCallerId: "traefik"
        );

        selected.Should().Be(ServiceToServiceDefaults.AuthenticationScheme);
    }

    private static string SelectScheme(
        Dictionary<string, StringValues> headers,
        bool fallbackToDefaultSchemeOnMissingHeader,
        string defaultScheme,
        string ingressCallerId = null
    )
    {
        const string PolicyScheme = "S2SOrDefault";

        using var provider = new ServiceCollection()
            .AddAuthentication()
            .AddServiceToServicePolicyScheme(
                defaultScheme: defaultScheme,
                ingressCallerId: ingressCallerId,
                fallbackToDefaultSchemeOnMissingHeader: fallbackToDefaultSchemeOnMissingHeader,
                policyScheme: PolicyScheme
            )
            .Services.BuildServiceProvider();

        var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<PolicySchemeOptions>>();
        var options = optionsMonitor.Get(PolicyScheme);
        var context = new DefaultHttpContext();
        foreach (var (key, value) in headers)
        {
            context.Request.Headers[key] = value;
        }

        return options.ForwardDefaultSelector?.Invoke(context)
            ?? throw new InvalidOperationException("Policy scheme selector should always resolve a target scheme.");
    }
}
