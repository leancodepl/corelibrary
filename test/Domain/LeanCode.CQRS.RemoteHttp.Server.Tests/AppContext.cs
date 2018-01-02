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

    public sealed class ObjContext
    {
        public AppContext SourceContext { get; set; }
    }

    public class ObjContextFromAppContextFactory : IObjectContextFromAppContextFactory<AppContext, ObjContext>
    {
        public ObjContext Create(AppContext appContext)
        {
            return new ObjContext { SourceContext = appContext };
        }
    }
}
