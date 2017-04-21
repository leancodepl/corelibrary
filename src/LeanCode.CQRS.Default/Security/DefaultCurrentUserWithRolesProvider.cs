using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Security.Exceptions;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.Default.Security
{
    class DefaultCurrentUserWithRoles : ICurrentUserWithRoles
    {
        public IReadOnlyList<Role> Roles { get; }
        public IReadOnlyList<string> Permissions { get; }

        public DefaultCurrentUserWithRoles(List<Role> roles)
        {
            Roles = roles;
            Permissions = Roles
                .SelectMany(r => r.Permissions)
                .Distinct()
                .ToList();
        }
    }

    public class DefaultCurrentUserWithRolesProvider : ICurrentUserWithRolesProvider
    {
        private IHttpContextAccessor contextAccessor;

        public DefaultCurrentUserWithRolesProvider(
            IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        public ICurrentUserWithRoles GetCurrentUser()
        {
            var claimsPrincipal = contextAccessor.HttpContext.User;

            var roles = ParseRolesFromClaims(claimsPrincipal);

            return new DefaultCurrentUserWithRoles(roles);
        }

        protected List<Role> ParseRolesFromClaims(ClaimsPrincipal claimsPrincipal)
        {
            return GetClaims(claimsPrincipal, "role")
                .Select(c => RoleRegistry.All.FirstOrDefault(role => role.Name == c.Value))
                .Where(role => role != null)
                .ToList();
        }

        protected IEnumerable<Claim> GetClaims(ClaimsPrincipal claimsPrincipal, string claimType)
        {
            if (claimsPrincipal == null || !claimsPrincipal.Identity.IsAuthenticated)
            {
                throw new InsufficientPermissionException("User is not authenticated");
            }

            return claimsPrincipal.FindAll(claimType);
        }
    }
}
