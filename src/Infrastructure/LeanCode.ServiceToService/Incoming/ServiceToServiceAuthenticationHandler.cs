using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LeanCode.ServiceToService.Incoming;

/// <summary>
/// Authenticates callers based on <c>LNCD-Caller-Id</c>.
/// Security depends on infrastructure-level header enforcement (for example service mesh or ingress middleware).
/// If the header is not enforced and can be set by arbitrary clients, caller identity can be spoofed.
/// S2S requests are additionally guarded by <c>S2SApiKey</c> as defense in depth.
/// </summary>
public partial class ServiceToServiceAuthenticationHandler(
    IOptionsMonitor<ServiceToServiceAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<ServiceToServiceAuthenticationOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (
            !TryGetSingleHeaderValue(
                ServiceToServiceConsts.S2SApiKeyHeaderName,
                out var s2SApiKey,
                out var multipleS2SApiKeys
            )
        )
        {
            LogMissingOrMultipleS2SApiKeyHeader(Logger, ServiceToServiceConsts.S2SApiKeyHeaderName);
            if (multipleS2SApiKeys)
            {
                return Task.FromResult(AuthenticateResult.Fail("Multiple S2S API key headers are not allowed."));
            }

            if (Options.RejectMissingS2SApiKey)
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing S2S API key header."));
            }

            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!VerifyApiKey(s2SApiKey, Options.S2SApiKey))
        {
            LogInvalidS2SApiKey(Logger);
            return Task.FromResult(AuthenticateResult.Fail("Invalid S2S API key."));
        }

        if (
            !TryGetSingleHeaderValue(
                ServiceToServiceConsts.CallerIdHeaderName,
                out var callerId,
                out var multipleCallerIds
            )
        )
        {
            LogMissingOrMultipleCallerIdHeader(Logger, ServiceToServiceConsts.CallerIdHeaderName);
            if (multipleCallerIds)
            {
                return Task.FromResult(AuthenticateResult.Fail("Multiple caller identity headers are not allowed."));
            }

            if (Options.RejectMissingCallerId)
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing caller identity header."));
            }

            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!Options.CallerRoles.TryGetValue(callerId, out var roles))
        {
            if (Options.RejectUnknownCallers)
            {
                LogUnknownServiceCaller(Logger, callerId);
                return Task.FromResult(AuthenticateResult.Fail($"Unknown service caller: {callerId}"));
            }

            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim> { new(Options.NameClaimType, callerId) };
        claims.AddRange(roles.Select(role => new Claim(Options.RoleClaimType, role)));

        var identity = new ClaimsIdentity(claims, Scheme.Name, Options.NameClaimType, Options.RoleClaimType);
        var ticket = new AuthenticationTicket(new(identity), Scheme.Name);

        LogAuthenticatedServiceCaller(Logger, callerId, string.Join(", ", roles));

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private bool TryGetSingleHeaderValue(string headerName, out string value, out bool multipleValues)
    {
        value = string.Empty;
        multipleValues = false;

        if (!Request.Headers.TryGetValue(headerName, out var headerValues))
        {
            return false;
        }

        if (headerValues.Count > 1)
        {
            multipleValues = true;
            return false;
        }

        var headerValue = headerValues[0];
        if (string.IsNullOrWhiteSpace(headerValue))
        {
            return false;
        }

        value = headerValue;
        return true;
    }

    private static bool VerifyApiKey(string left, string right)
    {
        var leftBytes = MemoryMarshal.AsBytes(left.AsSpan());
        var rightBytes = MemoryMarshal.AsBytes(right.AsSpan());

        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Missing or multiple {Header} headers")]
    private static partial void LogMissingOrMultipleS2SApiKeyHeader(ILogger logger, string header);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Invalid S2S API key")]
    private static partial void LogInvalidS2SApiKey(ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Missing or multiple {Header} headers")]
    private static partial void LogMissingOrMultipleCallerIdHeader(ILogger logger, string header);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Unknown service caller: {CallerId}")]
    private static partial void LogUnknownServiceCaller(ILogger logger, string callerId);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Debug,
        Message = "Authenticated s2s caller {CallerId} with roles [{Roles}]"
    )]
    private static partial void LogAuthenticatedServiceCaller(ILogger logger, string callerId, string roles);
}
