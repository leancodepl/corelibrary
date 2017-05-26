using LeanCode.CQRS.Security;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.Default.Security
{
    public class DefaultAuthorizer : IAuthorizer
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<DefaultAuthorizer>();

        // This is bad, we need to switch to context-based information that is
        // passed by the caller of executor and do not rely on HttpContextAccessor.
        // That way we may be able to use the CQRS infrastructure outside of HTTP
        // context (async command queues) and avoid many errors related to not-so-perfect
        // human memory (e.g. latest change - handling unauthorized users correctly)
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IAuthorizerResolver authorizerResolver;

        public DefaultAuthorizer(IHttpContextAccessor httpContextAcessor, IAuthorizerResolver authorizerResolver)
        {
            this.httpContextAccessor = httpContextAcessor;
            this.authorizerResolver = authorizerResolver;
        }

        public AuthorizationResult CheckIfAuthorized<T>(T obj)
        {
            var customAuthorizers = AuthorizeWhenAttribute.GetAuthorizers(obj);

            if (customAuthorizers.Count > 0)
            {
                var curr = httpContextAccessor.HttpContext;
                if (curr.User == null || curr.User.Identity == null || !curr.User.Identity.IsAuthenticated)
                {
                    return AuthorizationResult.Unauthenticated;
                }
            }

            foreach (var customAuthorizerDefinition in customAuthorizers)
            {
                var customAuthorizer = authorizerResolver.FindAuthorizer(customAuthorizerDefinition.Authorizer);
                if (!customAuthorizer.CheckIfAuthorized(obj, customAuthorizerDefinition.CustomData))
                {
                    logger.Warning("Authorizer {Authorizer} failed to authorize the user to run {@Object}", customAuthorizer.GetType().FullName, obj);
                    return AuthorizationResult.InsufficientPermission;
                }
            }

            return AuthorizationResult.Authorized;
        }
    }
}
