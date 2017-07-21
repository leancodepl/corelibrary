using System.Linq;
using System.Security.Claims;

namespace LeanCode.CQRS.Security
{
    static class ClaimsPrincipalExtensions
    {
        public static bool HasPermission(
            this ClaimsPrincipal claimsPrincipal,
            RoleRegistry registry,
            params string[] permissions)
        {
            foreach (var role in registry.All)
            {
                if (claimsPrincipal.IsInRole(role.Name))
                {
                    if (role.Permissions.Any(role.Permissions.Contains))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
