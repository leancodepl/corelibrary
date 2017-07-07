using System.Threading.Tasks;
using LeanCode.CQRS.Security;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.Default.Security
{
    public class DefaultPermissionAuthorizer : PermissionAuthorizer
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
                logger.Warning("User does not have sufficient permissions ({Permissions}) to run {@Object}",
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
