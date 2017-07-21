using System.Security.Claims;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Security
{
    public interface ISecurityContext : IPipelineContext
    {
        ClaimsPrincipal User { get; }
    }
}
