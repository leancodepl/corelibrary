using System.Security.Claims;
using LeanCode.IntegrationTestHelpers;

namespace LeanCode.IntegrationTests.App;

public static class AuthConfig
{
    public static readonly Guid UserId = Guid.NewGuid();

    public static readonly ClaimsPrincipal User =
        new(
            new ClaimsIdentity(
                new Claim[] { new("sub", UserId.ToString()), new("role", "user") },
                TestAuthenticationHandler.SchemeName,
                "sub",
                "role"
            )
        );
}
