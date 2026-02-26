using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LeanCode.ServiceToService.Incoming;

/// <summary>
/// Authenticates callers based on <c>LNCD-Caller-Id</c>.
/// Security depends on infrastructure-level header enforcement (for example service mesh or ingress middleware).
/// If the header is not enforced and can be set by arbitrary clients, caller identity can be spoofed.
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
            !Request.Headers.TryGetValue(ServiceToServiceDefaults.CallerIdHeaderName, out var values)
            || string.IsNullOrWhiteSpace(values.ToString())
        )
        {
            if (Options.RejectMissingCallerId)
            {
                LogMissingCallerIdHeader(Logger, ServiceToServiceDefaults.CallerIdHeaderName);
                return Task.FromResult(AuthenticateResult.Fail("Missing caller identity header."));
            }

            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var callerId = values.ToString();

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

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Missing {Header} header")]
    private static partial void LogMissingCallerIdHeader(ILogger logger, string header);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Unknown service caller: {CallerId}")]
    private static partial void LogUnknownServiceCaller(ILogger logger, string callerId);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Authenticated s2s caller {CallerId} with roles [{Roles}]"
    )]
    private static partial void LogAuthenticatedServiceCaller(ILogger logger, string callerId, string roles);
}
