using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace LeanCode.CQRS.Security
{
    static class ClaimsPrincipalExtensions
    {
        public static bool HasPermission(this ClaimsPrincipal claimsPrincipal, params string[] permissions)
        {
            var allPermissions = new HashSet<string>(RoleRegistry.All
                .Where(role => claimsPrincipal.IsInRole(role.Name))
                .SelectMany(role => role.Permissions));

            return permissions.Any(allPermissions.Contains);
        }
    }
}
