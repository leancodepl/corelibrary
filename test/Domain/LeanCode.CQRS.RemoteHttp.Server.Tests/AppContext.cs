using System.Security.Claims;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    public sealed class AppContext
    {
        public ClaimsPrincipal User { get; }

        public AppContext(ClaimsPrincipal user)
        {
            User = user;
        }
    }
}
