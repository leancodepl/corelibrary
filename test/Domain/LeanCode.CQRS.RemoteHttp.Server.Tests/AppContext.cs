using System.Security.Claims;
using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests;

public sealed class AppContext : IPipelineContext
{
    IPipelineScope IPipelineContext.Scope { get; set; } = null!;
    public CancellationToken CancellationToken => CancellationToken.None;

    public ClaimsPrincipal User { get; }

    public AppContext(ClaimsPrincipal user)
    {
        User = user;
    }

    public static AppContext FromHttp(HttpContext httpContext) => new(httpContext.User);
}
