using System.Linq;
using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.Default.Security
{
    public class DefaultAuthorizer : IAuthorizer
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<DefaultAuthorizer>();

        private readonly ICurrentUserWithRolesProvider currentUserProvider;
        private readonly IAuthorizerResolver authorizerResolver;

        public DefaultAuthorizer(
            ICurrentUserWithRolesProvider currentUserProvider,
            IAuthorizerResolver authorizerResolver)
        {
            this.currentUserProvider = currentUserProvider;
            this.authorizerResolver = authorizerResolver;
        }

        public bool CheckIfAuthorized<T>(T obj)
        {
            return CheckIfSufficientRole(obj) && CheckCustomAccess(obj);
        }

        private bool CheckIfSufficientRole<T>(T obj)
        {
            var permissions = AuthorizeWithPermissionAttribute.GetPerrmissions(obj);
            if (permissions.Any())
            {
                var user = currentUserProvider.GetCurrentUser();
                if (!permissions.Any(permission => user.Permissions.Contains(permission)))
                {
                    logger.Warning("User {@User} does not have sufficient roles ({Roles}) to run {@Object}", user, permissions, obj);
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
