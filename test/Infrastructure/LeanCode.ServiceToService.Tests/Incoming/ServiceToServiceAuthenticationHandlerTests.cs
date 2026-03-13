using System.Collections.Frozen;
using System.Security.Claims;
using FluentAssertions;
using FluentAssertions.Execution;
using LeanCode.CQRS.Security;
using LeanCode.ServiceToService.Incoming;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace LeanCode.ServiceToService.Tests.Incoming;

public class ServiceToServiceAuthenticationHandlerTests
{
    private const string ValidS2SApiKey = "project-dev-api-key";

    [Fact]
    public async Task Returns_failure_when_s2s_api_key_is_missing_and_rejection_is_enabled()
    {
        var handler = ConfigureServices();

        var result = await AuthenticateAsync(handler, []);

        using var _ = new AssertionScope();
        result.None.Should().BeFalse();
        result.Ticket.Should().BeNull();
        result.Principal.Should().BeNull();
        result.Failure.Should().NotBeNull();
    }

    [Fact]
    public async Task Returns_none_when_s2s_api_key_is_missing_and_rejection_is_disabled()
    {
        var handler = ConfigureServices(options => options.RejectMissingS2SApiKey = false);

        var result = await AuthenticateAsync(handler, []);

        using var _ = new AssertionScope();
        result.None.Should().BeTrue();
        result.Failure.Should().BeNull();
        result.Ticket.Should().BeNull();
        result.Principal.Should().BeNull();
    }

    [Fact]
    public async Task Returns_failure_when_s2s_api_key_is_invalid_and_rejection_is_enabled()
    {
        var handler = ConfigureServices();

        var result = await AuthenticateAsync(
            handler,
            new() { [ServiceToServiceDefaults.S2SApiKeyHeaderName] = "invalid-api-key" }
        );

        using var _ = new AssertionScope();
        result.None.Should().BeFalse();
        result.Ticket.Should().BeNull();
        result.Principal.Should().BeNull();
        result.Failure.Should().NotBeNull();
    }

    [Fact]
    public async Task Returns_failure_when_caller_id_header_is_missing_and_rejection_is_enabled()
    {
        var handler = ConfigureServices();

        var result = await AuthenticateAsync(
            handler,
            new() { [ServiceToServiceDefaults.S2SApiKeyHeaderName] = ValidS2SApiKey }
        );

        using var _ = new AssertionScope();
        result.None.Should().BeFalse();
        result.Ticket.Should().BeNull();
        result.Principal.Should().BeNull();
        result.Failure.Should().NotBeNull();
    }

    [Fact]
    public async Task Returns_none_when_caller_id_header_is_missing_and_rejection_is_disabled()
    {
        var handler = ConfigureServices(options => options.RejectMissingCallerId = false);

        var result = await AuthenticateAsync(
            handler,
            new() { [ServiceToServiceDefaults.S2SApiKeyHeaderName] = ValidS2SApiKey }
        );

        using var _ = new AssertionScope();
        result.None.Should().BeTrue();
        result.Failure.Should().BeNull();
        result.Ticket.Should().BeNull();
        result.Principal.Should().BeNull();
    }

    [Fact]
    public async Task Returns_failure_for_unknown_caller_when_rejection_is_enabled()
    {
        var handler = ConfigureServices();

        var result = await AuthenticateAsync(
            handler,
            new()
            {
                [ServiceToServiceDefaults.S2SApiKeyHeaderName] = ValidS2SApiKey,
                [ServiceToServiceDefaults.CallerIdHeaderName] = "unknown-service",
            }
        );

        using var _ = new AssertionScope();
        result.None.Should().BeFalse();
        result.Ticket.Should().BeNull();
        result.Principal.Should().BeNull();
        result.Failure.Should().NotBeNull();
    }

    [Fact]
    public async Task Returns_failure_when_multiple_caller_id_headers_are_present()
    {
        var handler = ConfigureServices();

        var result = await AuthenticateAsync(
            handler,
            new()
            {
                [ServiceToServiceDefaults.S2SApiKeyHeaderName] = ValidS2SApiKey,
                [ServiceToServiceDefaults.CallerIdHeaderName] = new StringValues([
                    "notifications-service",
                    "another-service",
                ]),
            }
        );

        using var _ = new AssertionScope();
        result.None.Should().BeFalse();
        result.Ticket.Should().BeNull();
        result.Principal.Should().BeNull();
        result.Failure.Should().NotBeNull();
    }

    [Fact]
    public async Task Returns_none_for_unknown_caller_when_rejection_is_disabled()
    {
        var handler = ConfigureServices(options => options.RejectUnknownCallers = false);

        var result = await AuthenticateAsync(
            handler,
            new()
            {
                [ServiceToServiceDefaults.S2SApiKeyHeaderName] = ValidS2SApiKey,
                [ServiceToServiceDefaults.CallerIdHeaderName] = "unknown-service",
            }
        );

        using var _ = new AssertionScope();
        result.None.Should().BeTrue();
        result.Failure.Should().BeNull();
        result.Ticket.Should().BeNull();
        result.Principal.Should().BeNull();
    }

    [Fact]
    public async Task Returns_success_with_subject_and_role_claims_for_known_caller_with_valid_s2s_api_key()
    {
        var handler = ConfigureServices(options =>
        {
            options.NameClaimType = "sub";
            options.RoleClaimType = "role";
            options.CallerRoles = new Dictionary<string, FrozenSet<string>>
            {
                ["notifications-service"] = ["system_notifications_service", "system_admin_panel"],
            }.ToFrozenDictionary();
        });

        var result = await AuthenticateAsync(
            handler,
            new()
            {
                [ServiceToServiceDefaults.S2SApiKeyHeaderName] = ValidS2SApiKey,
                [ServiceToServiceDefaults.CallerIdHeaderName] = "notifications-service",
            }
        );

        using var _ = new AssertionScope();
        result.Succeeded.Should().BeTrue();
        result.None.Should().BeFalse();
        result.Failure.Should().BeNull();
        result.Ticket.Should().NotBeNull();
        result.Principal.Should().NotBeNull();

        result
            .Principal!.Claims.Should()
            .ContainSingle(c => c.Type == "sub")
            .Which.Value.Should()
            .Be("notifications-service");

        var roleClaims = result.Principal.Claims.Where(c => c.Type == "role");
        roleClaims.Should().HaveCount(2);
        roleClaims.Select(c => c.Value).Should().BeEquivalentTo("system_notifications_service", "system_admin_panel");
    }

    private static ServiceToServiceAuthenticationHandler ConfigureServices(
        Action<ServiceToServiceAuthenticationOptions> configure = null
    )
    {
        var roleRegistry = new RoleRegistry([
            new TestRoleRegistration([new("system_notifications_service"), new("system_admin_panel")]),
        ]);

        var services = new ServiceCollection();
        services
            .AddLogging()
            .AddSingleton(roleRegistry)
            .AddAuthentication()
            .AddServiceToService(options =>
            {
                options.RoleClaimType = ClaimTypes.Role;
                options.S2SApiKey = ValidS2SApiKey;
                options.CallerRoles = new Dictionary<string, FrozenSet<string>>
                {
                    ["notifications-service"] = ["system_notifications_service"],
                }.ToFrozenDictionary();
                configure?.Invoke(options);
            });

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<ServiceToServiceAuthenticationHandler>();
    }

    private static async Task<AuthenticateResult> AuthenticateAsync(
        ServiceToServiceAuthenticationHandler handler,
        Dictionary<string, StringValues> headers
    )
    {
        var features = new FeatureCollection();
        features.Set<IHttpRequestFeature>(new HttpRequestFeature { Headers = new HeaderDictionary(headers) });
        var context = new DefaultHttpContext(features);

        await handler.InitializeAsync(
            new(
                ServiceToServiceDefaults.AuthenticationScheme,
                ServiceToServiceDefaults.AuthenticationScheme,
                handler.GetType()
            ),
            context
        );

        return await handler.AuthenticateAsync();
    }

    private sealed class TestRoleRegistration : IRoleRegistration
    {
        public IEnumerable<Role> Roles { get; }

        public TestRoleRegistration(IEnumerable<Role> roles)
        {
            Roles = roles;
        }
    }
}
