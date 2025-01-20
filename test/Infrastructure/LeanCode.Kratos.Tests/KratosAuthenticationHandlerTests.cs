using System.Net;
using LeanCode.Kratos.Client.Api;
using LeanCode.Kratos.Client.Client;
using LeanCode.Kratos.Client.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using static LeanCode.Kratos.Client.Model.KratosSessionAuthenticationMethod;

namespace LeanCode.Kratos.Tests;

public class KratosAuthenticationHandlerTests
{
    private sealed record Traits(string Email);

    private sealed record MetadataPublic(bool IsAdmin);

    private readonly KratosSession session = new(
        active: true,
        authenticatedAt: DateTime.UnixEpoch.AddSeconds(1),
        authenticationMethods: new(
            new(1)
            {
                new(KratosAuthenticatorAssuranceLevel.Aal1, DateTime.UnixEpoch.AddSeconds(1), MethodEnum.Password),
            }
        ),
        authenticatorAssuranceLevel: KratosAuthenticatorAssuranceLevel.Aal1,
        expiresAt: DateTime.UnixEpoch.AddSeconds(3),
        id: Guid.NewGuid().ToString(),
        identity: new(
            new(
                id: Guid.NewGuid().ToString(),
                metadataPublic: new MetadataPublic(true),
                schemaId: "user",
                schemaUrl: "https://auth.local.lncd.pl/schemas/dXNlcg",
                state: KratosIdentity.StateEnum.Active,
                traits: new Traits("test@leancode.pl")
            )
        ),
        issuedAt: DateTime.UnixEpoch.AddSeconds(2)
    );

    private readonly IToSessionApiResponse sessionResponse;

    public KratosAuthenticationHandlerTests()
    {
        sessionResponse = Substitute.For<IToSessionApiResponse>();
        sessionResponse.Ok().Returns(session);
    }

    [Fact]
    public async Task Returns_none_result_if_credentials_are_missing()
    {
        var (handler, api) = ConfigureServices();

        api.ToSessionOrDefaultAsync(
                Arg.Any<Option<string>>(),
                Arg.Any<Option<string>>(),
                Arg.Any<Option<string>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(sessionResponse));

        var result = await AuthenticateAsync(handler, new() { ["Cookie"] = "not_a_kratos_session=foo" });

        Assert.False(result.Succeeded);
        Assert.True(result.None);
    }

    [Fact]
    public async Task Returns_success_for_valid_cookie()
    {
        var (handler, api) = ConfigureServices();

        api.ToSessionAsync(default, $"{KratosDefaults.SessionCookieName}=foo", default, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(sessionResponse));

        var result = await AuthenticateAsync(handler, new() { ["Cookie"] = "ory_kratos_session=foo" });

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("X-Session-Token", "foo")]
    [InlineData("Authorization", "Bearer foo")]
    public async Task Returns_success_for_valid_token(string headerName, string headerValue)
    {
        var (handler, api) = ConfigureServices();

        api.ToSessionAsync("foo", default, default, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(sessionResponse));

        var result = await AuthenticateAsync(handler, new() { [headerName] = headerValue });

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Returns_failure_for_invalid_cookie()
    {
        var (handler, api) = ConfigureServices();

        api.ToSessionAsync(
                Arg.Any<Option<string>>(),
                Arg.Any<Option<string>>(),
                Arg.Any<Option<string>>(),
                Arg.Any<CancellationToken>()
            )
            .Throws(
                new ApiException(
                    "The request could not be authorized",
                    HttpStatusCode.Unauthorized,
                    @"{""error"":{""code"":401,""message"":""The request could not be authorized""}}"
                )
            );

        var result = await AuthenticateAsync(handler, new() { ["Cookie"] = "ory_kratos_session=foo" });

        Assert.False(result.Succeeded);
        var exception = Assert.IsType<ApiException>(result.Failure);
        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
    }

    [Theory]
    [InlineData("X-Session-Token", "foo")]
    [InlineData("Authorization", "Bearer foo")]
    public async Task Returns_failure_for_invalid_token(string headerName, string headerValue)
    {
        var (handler, api) = ConfigureServices();

        api.ToSessionAsync(
                Arg.Any<Option<string>>(),
                Arg.Any<Option<string>>(),
                Arg.Any<Option<string>>(),
                Arg.Any<CancellationToken>()
            )
            .Throws(
                new ApiException(
                    "The request could not be authorized",
                    HttpStatusCode.Unauthorized,
                    @"{""error"":{""code"":401,""message"":""The request could not be authorized""}}"
                )
            );

        var result = await AuthenticateAsync(handler, new() { [headerName] = headerValue });

        Assert.False(result.Succeeded);
        var exception = Assert.IsType<ApiException>(result.Failure);
        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
    }

    [Fact]
    public async Task Rejects_inactive_sessions()
    {
        var (handler, api) = ConfigureServices();

        session.Active = false;

        api.ToSessionAsync("foo", default, default, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(sessionResponse));

        var result = await AuthenticateAsync(handler, new() { ["X-Session-Token"] = "foo" });

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Rejects_inactive_identities_when_configured_to_do_so()
    {
        var (handler, api) = ConfigureServices(cfg => cfg.AllowInactiveIdentities = false);

        session.Identity.State = KratosIdentity.StateEnum.Inactive;

        api.ToSessionAsync("foo", default, default, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(sessionResponse));

        var result = await AuthenticateAsync(handler, new() { ["X-Session-Token"] = "foo" });

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Accepts_inactive_identities_when_configured_to_do_so()
    {
        var (handler, api) = ConfigureServices(cfg => cfg.AllowInactiveIdentities = true);

        session.Identity.State = KratosIdentity.StateEnum.Inactive;

        api.ToSessionAsync("foo", default, default, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(sessionResponse));

        var result = await AuthenticateAsync(handler, new() { ["X-Session-Token"] = "foo" });

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Maps_session_and_identity_properties_to_principals_claims()
    {
        var (handler, api) = ConfigureServices(cfg =>
        {
            cfg.RoleClaimType = "role";
            cfg.NameClaimType = "sub";

            cfg.ClaimsExtractor = (s, o, c) =>
            {
                switch (s.Identity.SchemaId)
                {
                    case "user":
                        c.Add(new(o.RoleClaimType, "user"));
                        break;
                }

                if (
                    s.Identity.Traits is Traits(var email)
                    && email.EndsWith("@leancode.pl", StringComparison.InvariantCultureIgnoreCase)
                )
                {
                    c.Add(new(o.RoleClaimType, "developer"));
                }

                if (s.Identity.MetadataPublic is Option<object> { Value: MetadataPublic { IsAdmin: true } })
                {
                    c.Add(new(o.RoleClaimType, "admin"));
                }
            };
        });

        api.ToSessionAsync("foo", default, default, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(sessionResponse));

        var result = await AuthenticateAsync(handler, new() { ["X-Session-Token"] = "foo" });

        Assert.True(result.Succeeded);

        Assert.Equivalent(
            new[]
            {
                ("iss", "https://auth.local.lncd.pl"),
                ("auth_time", "1"),
                ("iat", "2"),
                ("exp", "3"),
                ("acr", "aal1"),
                ("amr", "password"),
                ("sub", session.Identity.Id),
                ("role", "user"),
                ("role", "developer"),
                ("role", "admin"),
            },
            result.Principal?.Claims.Select(c => (c.Type, c.Value)),
            strict: false
        );
    }

    private static (KratosAuthenticationHandler, IFrontendApi) ConfigureServices(
        Action<KratosAuthenticationOptions> configureAuthenticationOptions = null
    )
    {
        var frontendApi = Substitute.For<IFrontendApi>();
        var services = new ServiceCollection();

        services
            .AddLogging(log => log.AddProvider(NullLoggerProvider.Instance))
            .AddSingleton(frontendApi)
            .AddAuthentication()
            .AddKratos(cfg =>
            {
                cfg.RoleClaimType = "role";
                cfg.NameClaimType = "sub";

                cfg.ClaimsExtractor = (_, _, _) => { };

                configureAuthenticationOptions?.Invoke(cfg);
            });

        var sp = services.BuildServiceProvider();

        return (sp.GetRequiredService<KratosAuthenticationHandler>(), frontendApi);
    }

    private static async Task<AuthenticateResult> AuthenticateAsync(
        KratosAuthenticationHandler authenticationHandler,
        Dictionary<string, StringValues> headers
    )
    {
        var features = new FeatureCollection();
        features.Set<IHttpRequestFeature>(new HttpRequestFeature() { Headers = new HeaderDictionary(headers) });
        var httpContext = new DefaultHttpContext(features);

        await authenticationHandler.InitializeAsync(
            new AuthenticationScheme("Kratos", "Kratos", authenticationHandler.GetType()),
            httpContext
        );

        return await authenticationHandler.AuthenticateAsync();
    }
}
