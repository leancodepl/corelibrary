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

    public sealed class ObjContextWoCtor
    {
        public AppContext SourceContext { get; }

        public ObjContextWoCtor(AppContext sourceContext)
        {
            SourceContext = sourceContext;
        }
    }

    public class ObjContextFromAppContextFactory : IObjectContextFromAppContextFactory<AppContext, ObjContext>
    {
        public ObjContext Create(AppContext appContext)
        {
            return new ObjContext { SourceContext = appContext };
        }
    }

    public class ObjContextWoCtorFromAppContextFactory : IObjectContextFromAppContextFactory<AppContext, ObjContextWoCtor>
    {
        public ObjContextWoCtor Create(AppContext appContext)
        {
            return new ObjContextWoCtor(appContext);
        }
    }
}
