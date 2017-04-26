using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.Default.Security
{
    public class DefaultAuthorizer : IAuthorizer
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<DefaultAuthorizer>();

        private readonly IAuthorizerResolver authorizerResolver;

        public DefaultAuthorizer(
            IAuthorizerResolver authorizerResolver)
        {
            this.authorizerResolver = authorizerResolver;
        }

        public bool CheckIfAuthorized<T>(T obj)
        {
            var customAuthorizers = AuthorizeWhenAttribute.GetAuthorizers(obj);
            foreach (var customAuthorizerDefinition in customAuthorizers)
            {
                var customAuthorizer = authorizerResolver.FindAuthorizer(customAuthorizerDefinition.Authorizer);
                if (!customAuthorizer.CheckIfAuthorized(obj, customAuthorizerDefinition.CustomData))
                {
                    logger.Warning("Authorizer {Authorizer} failed to authorize the user to run {@Object}", customAuthorizer.GetType().FullName, obj);
                    return false;
                }
            }

            return true;
        }
    }
}
