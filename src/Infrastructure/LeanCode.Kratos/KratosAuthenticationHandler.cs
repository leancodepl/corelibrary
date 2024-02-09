using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ory.Kratos.Client.Api;
using Ory.Kratos.Client.Model;
using static Ory.Kratos.Client.Model.KratosSessionAuthenticationMethod;

namespace LeanCode.Kratos;

public class KratosAuthenticationHandler<TOptions> : AuthenticationHandler<TOptions>
    where TOptions : KratosAuthenticationOptions, new()
{
    private static readonly ImmutableDictionary<MethodEnum, string> AuthenticationMethods =
        ExtractEnumNames<MethodEnum>();

    private static readonly ImmutableDictionary<KratosAuthenticatorAssuranceLevel, string> AssuranceLevels =
        ExtractEnumNames<KratosAuthenticatorAssuranceLevel>();

    private readonly IFrontendApi api;

    public KratosAuthenticationHandler(
        IOptionsMonitor<TOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IFrontendApi api
    )
        : base(options, logger, encoder)
    {
        this.api = api;
    }

    [SuppressMessage("?", "CA1031", Justification = "Exception is returned to the caller, wrapped in Fail result.")]
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var session = await GetSessionAsync();

            if (session is null)
            {
                return AuthenticateResult.NoResult();
            }
            else if (!session.Active)
            {
                return AuthenticateResult.Fail("Session is not active.");
            }
            else if (session.Identity is null)
            {
                return AuthenticateResult.Fail("Session does not contain an identity.");
            }
            if (!Options.AllowInactiveIdentities && session.Identity.State != KratosIdentityState.Active)
            {
                return AuthenticateResult.Fail("Identity is not active.");
            }

            var claims = ExtractClaims(session);

            return AuthenticateResult.Success(
                new(
                    new(new ClaimsIdentity(claims, Scheme.Name, Options.NameClaimType, Options.RoleClaimType)),
                    Scheme.Name
                )
            );
        }
        catch (Exception e)
        {
            return AuthenticateResult.Fail(e);
        }
    }

    protected virtual Task<KratosSession?> GetSessionAsync()
    {
        if (Request.Headers.TryGetValue("X-Session-Token", out var token) && !string.IsNullOrEmpty(token))
        {
            return api.ToSessionAsync(xSessionToken: token);
        }
        else if (
            AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out var value)
            && string.Equals(value.Scheme, "bearer", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrEmpty(value.Parameter)
        )
        {
            return api.ToSessionAsync(xSessionToken: value.Parameter);
        }
        else if (
            Request.Cookies.TryGetValue(Options.SessionCookieName, out var cookie) && !string.IsNullOrEmpty(cookie)
        )
        {
            return api.ToSessionAsync(cookie: $"{Options.SessionCookieName}={cookie}");
        }
        else
        {
            return Task.FromResult(null as KratosSession);
        }
    }

    protected virtual List<Claim> ExtractClaims(KratosSession session)
    {
        var claims = new List<Claim>
        {
            new(Options.NameClaimType, session.Identity.Id),
            new("iss", api.Configuration.BasePath),
            new("iat", ToUnixTimeSecondsString(session.IssuedAt)),
            new("exp", ToUnixTimeSecondsString(session.ExpiresAt)),
            new("auth_time", ToUnixTimeSecondsString(session.AuthenticatedAt)),
        };

        if (AssuranceLevels.TryGetValue(session.AuthenticatorAssuranceLevel, out var aal))
        {
            claims.Add(new("acr", aal));
        }

        foreach (var am in session.AuthenticationMethods)
        {
            if (am.Method is { } method && AuthenticationMethods.TryGetValue(method, out var methodName))
            {
                claims.Add(new("amr", methodName));
            }
        }

        Options.ClaimsExtractor(session, Options, claims);

        return claims;
    }

    private static string ToUnixTimeSecondsString(DateTime value) =>
        new DateTimeOffset(value).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);

    private static ImmutableDictionary<T, string> ExtractEnumNames<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T
    >()
        where T : struct, Enum
    {
        return Enum.GetValues<T>()
            .Distinct()
            .Select(v =>
                (
                    Value: v,
                    Name: (
                        typeof(T)
                            .GetField(v.ToString(), BindingFlags.Public | BindingFlags.Static)
                            ?.GetCustomAttribute(typeof(EnumMemberAttribute)) as EnumMemberAttribute
                    )?.Value
                )
            )
            .Where(t => t.Name is not null)
            .ToImmutableDictionary(t => t.Value, t => t.Name!);
    }
}

public class KratosAuthenticationHandler : KratosAuthenticationHandler<KratosAuthenticationOptions>
{
    public KratosAuthenticationHandler(
        IOptionsMonitor<KratosAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IFrontendApi api
    )
        : base(options, logger, encoder, api) { }
}
