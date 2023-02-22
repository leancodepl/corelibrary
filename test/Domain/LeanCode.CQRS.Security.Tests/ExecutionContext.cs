using System.Security.Claims;
using System.Threading;
using LeanCode.CQRS.Security;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default.Tests.Security;

public class ExecutionContext : ISecurityContext
{
    public IPipelineScope Scope { get; set; }
    public ClaimsPrincipal User { get; set; }
    public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
}
