using System.Threading.Tasks;

namespace LeanCode.CQRS.Security
{
    public class DefaultPermissionAuthorizer : CustomAuthorizer<ISecurityContext, object, string[]>, HasPermissions
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<DefaultPermissionAuthorizer>();

        private readonly RoleRegistry registry;

        public DefaultPermissionAuthorizer(RoleRegistry registry)
        {
            this.registry = registry;
        }

        protected override Task<bool> CheckIfAuthorizedAsync(
            ISecurityContext appContext,
            object obj,
            string[]? permissions = null)
        {
            if (!appContext.User.HasPermission(registry, permissions ?? System.Array.Empty<string>()))
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
