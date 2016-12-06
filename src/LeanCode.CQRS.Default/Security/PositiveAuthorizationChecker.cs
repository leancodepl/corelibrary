using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.Default.Security
{
    public class PositiveAuthorizationChecker : IAuthorizationChecker
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PositiveAuthorizationChecker>();

        public bool CheckIfAuthorized<T>(T obj)
        {
            logger.Verbose("Skipping authorization for object {@Object}", obj);
            return true;
        }
    }
}
