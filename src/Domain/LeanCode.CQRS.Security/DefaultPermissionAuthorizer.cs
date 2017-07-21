using System.Security.Claims;
using System.Threading.Tasks;

namespace LeanCode.CQRS.Security
{
    public class DefaultPermissionAuthorizer : CustomAuthorizer<object, string[]>, HasPermissions
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<DefaultPermissionAuthorizer>();

        private readonly RoleRegistry registry;

        public DefaultPermissionAuthorizer(RoleRegistry registry)
        {
            this.registry = registry;
        }

        public override Task<bool> CheckIfAuthorized(
            ClaimsPrincipal user, object obj, string[] permissions = null)
        {
            if (!user.HasPermission(registry, permissions))
            {
                logger.Warning(
                    "User does not have sufficient permissions ({Permissions}) to run {@Object}",
                    permissions, obj);
                return Task.FromResult(false);
            }
            else
            {
                return Task.FromResult(true);
            }
        }
    }
}
