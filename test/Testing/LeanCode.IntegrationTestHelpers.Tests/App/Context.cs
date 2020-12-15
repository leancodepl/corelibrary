using System.Threading;
using LeanCode.Pipelines;

namespace LeanCode.IntegrationTestHelpers.Tests.App
{
    public class Context : IPipelineContext
    {
        IPipelineScope IPipelineContext.Scope { get; set; } = default!;
        CancellationToken IPipelineContext.CancellationToken => default;
    }
}
