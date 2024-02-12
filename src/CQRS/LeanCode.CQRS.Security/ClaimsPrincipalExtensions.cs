using System.Linq;
using System.Security.Claims;

namespace LeanCode.CQRS.Security;

public static class ClaimsPrincipalExtensions
{
    public static bool HasPermission(
        this ClaimsPrincipal claimsPrincipal,
        RoleRegistry registry,
        params string[] permissions
    )
    {
        return registry.All.Any(role =>
            claimsPrincipal.IsInRole(role.Name) && permissions.Any(role.Permissions.Contains)
        );
    }
}
