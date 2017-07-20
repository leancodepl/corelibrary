using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.Security
{
    public class DefaultPermissionAuthorizer : CustomAuthorizer<object, string[]>, HasPermissions
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<DefaultPermissionAuthorizer>();

        private readonly IHttpContextAccessor httpContextAccessor;

        public DefaultPermissionAuthorizer(
            IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public override Task<bool> CheckIfAuthorized(object obj, string[] permissions = null)
        {
            if (!httpContextAccessor.HttpContext.User.HasPermission(permissions))
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
