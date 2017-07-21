using System.Security.Claims;
using LeanCode.CQRS.Security;
using LeanCode.Pipelines;

namespace LeanCode.Domain.Default.Tests.Security
{
    public class ExecutionContext : ISecurityContext
    {
        public IPipelineScope Scope { get; set; }

        public ClaimsPrincipal User { get; set; }
    }
}
