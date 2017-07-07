using System.Threading.Tasks;
using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.Default.Security
{
    public class PositiveAuthorizer : IAuthorizer
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PositiveAuthorizer>();

        public Task<AuthorizationResult> CheckIfAuthorized<T>(T obj)
        {
            logger.Verbose("Skipping authorization for object {@Object}", obj);
            return Task.FromResult(AuthorizationResult.Authorized);
        }
    }
}
