using LeanCode.Contracts;
using LeanCode.CQRS.OutputCaching.BasePolicies;
using Microsoft.AspNetCore.OutputCaching;

namespace LeanCode.CQRS.OutputCaching.Tests;

internal sealed class TestQuery : IQuery<TestResult>;

internal sealed class TestResult;

internal sealed class TestQueryOCP : CQRSOutputCachePolicy<TestQuery>
{
    public override ValueTask CacheRequestCoreAsync(OutputCacheContext context, CancellationToken cancellation) =>
        ValueTask.CompletedTask;
}

internal sealed class NonCQRSOCP : IOutputCachePolicy
{
    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation) =>
        throw new NotImplementedException();

    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation) =>
        throw new NotImplementedException();

    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation) =>
        throw new NotImplementedException();
}

internal sealed class RandomObjectOCP : CQRSOutputCachePolicy<string>
{
    public override ValueTask CacheRequestCoreAsync(OutputCacheContext context, CancellationToken cancellation) =>
        ValueTask.CompletedTask;
}

internal sealed class TestOperation : IOperation<TestOperationResult>;

internal sealed class TestOperationResult;

internal sealed class TestOperationOCP : CQRSOutputCachePolicy<TestOperation>
{
    public override ValueTask CacheRequestCoreAsync(OutputCacheContext context, CancellationToken cancellation) =>
        ValueTask.CompletedTask;
}

internal sealed class InvalidOCP;

internal sealed class TestCommand : ICommand;

internal sealed class TestHandler;
