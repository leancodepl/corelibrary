using System.Linq;
using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.Default.Security
{
    public class DefaultAuthorizationChecker : IAuthorizationChecker
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<DefaultAuthorizationChecker>();

        private readonly ICurrentUserWithRolesProvider currentUserProvider;
        private readonly IAuthorizationCheckerResolver authorizationCheckerResolver;

        public DefaultAuthorizationChecker(
            ICurrentUserWithRolesProvider currentUserProvider,
            IAuthorizationCheckerResolver authorizationCheckerResolver)
        {
            this.currentUserProvider = currentUserProvider;
            this.authorizationCheckerResolver = authorizationCheckerResolver;
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
            var customAccessCheckers = AuthorizeWithCheckerAttribute.GetAuthorizationCheckers(obj);
            foreach (var customAccessCheckerType in customAccessCheckers)
            {
                var customAccessChecker = authorizationCheckerResolver.FindAuthorizationChecker(customAccessCheckerType);
                if (!customAccessChecker.CheckIfAuthorized(obj))
                {
                    logger.Warning("Checker {Chcker} failed to authorize the user to run {@Object}", customAccessChecker.GetType().FullName, obj);
                    return false;
                }
            }

            return true;
        }
    }
}
