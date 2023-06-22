using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LeanCode.IntegrationTestHelpers;

public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock
    )
        : base(options, logger, encoder, clock) { }

    [SuppressMessage("?", "CA1031", Justification = "Method is an exception boundary")]
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var base64Principal = TryGetBase64Principal();
        if (base64Principal is null)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        try
        {
            var principal = DeserializePrincipal(base64Principal);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception e)
        {
            return Task.FromResult(AuthenticateResult.Fail(e));
        }
    }

    private string? TryGetBase64Principal()
    {
        var rawAuth = Request.Headers.Authorization;
        _ = AuthenticationHeaderValue.TryParse(rawAuth, out var auth);

        return auth?.Scheme == Scheme.Name ? auth.Parameter : null;
    }

    public static string SerializePrincipal(ClaimsPrincipal principal)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        principal.WriteTo(writer);
        return Convert.ToBase64String(ms.ToArray());
    }

    public static ClaimsPrincipal DeserializePrincipal(string base64)
    {
        using var ms = new MemoryStream(Convert.FromBase64String(base64));
        using var reader = new BinaryReader(ms);

        return new ClaimsPrincipal(reader);
    }
}

public static class TestAuthenticationHandlerExtensions
{
    public static AuthenticationBuilder AddTestAuthenticationHandler(
        this AuthenticationBuilder builder,
        Action<AuthenticationSchemeOptions>? config = null
    )
    {
        return builder.AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
            TestAuthenticationHandler.SchemeName,
            config
        );
    }
}
