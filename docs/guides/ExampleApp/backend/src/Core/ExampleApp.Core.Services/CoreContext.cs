using System.Security.Claims;
using ExampleApp.Core.Contracts;
using LeanCode.CQRS.Security;
using LeanCode.Pipelines;
using Microsoft.AspNetCore.Http;

namespace ExampleApp.Core.Services;

public class CoreContext : ISecurityContext
{
    IPipelineScope IPipelineContext.Scope { get; set; } = null!;

    public ClaimsPrincipal User { get; }
    public CancellationToken CancellationToken { get; }

    private CoreContext(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        User = user;
        CancellationToken = cancellationToken;
    }

    public static CoreContext FromHttp(HttpContext httpContext)
    {
        return new CoreContext(httpContext.User, httpContext.RequestAborted);
    }

    private static CoreContext ForTests(Guid userId, string role)
    {
        var claims = new[]
        {
            new Claim(Auth.KnownClaims.UserId, userId.ToString()),
            new Claim(Auth.KnownClaims.Role, role),
        };

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                claims: claims,
                authenticationType: "internal",
                nameType: Auth.KnownClaims.UserId,
                roleType: Auth.KnownClaims.Role
            )
        );

        return new CoreContext(user, default);
    }

    private static Guid ParseUserClaim(ClaimsPrincipal? user, string claimType)
    {
        if (user?.Identity?.IsAuthenticated ?? false)
        {
            var str = user.FindFirstValue(claimType);
            _ = Guid.TryParse(str, out var res);
            return res;
        }
        else
        {
            return Guid.Empty;
        }
    }
}
