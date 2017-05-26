using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.Default.Security
{
    public class PositiveAuthorizer : IAuthorizer
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PositiveAuthorizer>();

        public AuthorizationResult CheckIfAuthorized<T>(T obj)
        {
            logger.Verbose("Skipping authorization for object {@Object}", obj);
            return AuthorizationResult.Authorized;
        }
    }
}
