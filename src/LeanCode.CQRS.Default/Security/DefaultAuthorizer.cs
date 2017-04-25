using System.Linq;
using LeanCode.CQRS.Security;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.Default.Security
{
    public class DefaultAuthorizer : IAuthorizer
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<DefaultAuthorizer>();

        private readonly IHttpContextAccessor contextAccessor;
        private readonly IAuthorizerResolver authorizerResolver;

        public DefaultAuthorizer(
            IHttpContextAccessor contextAccessor,
            IAuthorizerResolver authorizerResolver)
        {
            this.authorizerResolver = authorizerResolver;
            this.contextAccessor = contextAccessor;
        }

        public bool CheckIfAuthorized<T>(T obj)
        {
            return CheckIfSufficientRole(obj) && CheckCustomAccess(obj);
        }

        private bool CheckIfSufficientRole<T>(T obj)
        {
            var permissions = AuthorizeWithPermissionAttribute.GetPermissions(obj);
            if (permissions.Any())
            {
                if (!contextAccessor.HttpContext.User.HasPermission(permissions))
                {
                    logger.Warning("User does not have sufficient roles ({Roles}) to run {@Object}", permissions, obj);
                    return false;
                }
            }

            return true;
        }

        private bool CheckCustomAccess<T>(T obj)
        {
            var customAuthorizers = AuthorizeWithAttribute.GetAuthorizers(obj);
            foreach (var customAuthorizerType in customAuthorizers)
            {
                var customAuthorizer = authorizerResolver.FindAuthorizer(customAuthorizerType);
                if (!customAuthorizer.CheckIfAuthorized(obj))
                {
                    logger.Warning("Authorizer {Authorizer} failed to authorize the user to run {@Object}", customAuthorizer.GetType().FullName, obj);
                    return false;
                }
            }

            return true;
        }
    }
}
