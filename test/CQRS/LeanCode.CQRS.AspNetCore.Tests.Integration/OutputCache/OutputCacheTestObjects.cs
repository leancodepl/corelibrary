using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.OutputCaching.BasePolicies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;

namespace LeanCode.CQRS.AspNetCore.Tests.Integration.OutputCache;

[CustomAuthorizeWhen]
public class CachedTestQuery : IQuery<TestQueryResult>, ICustomAuthorizerParams
{
    public int X { get; set; }
    public int Y { get; set; }
    public string? VaryByValue { get; set; }
    public bool FailAuthorization { get; set; }
}

[CustomAuthorizeWhen]
public class CachedTestOperation : IOperation<TestOperationResult>, ICustomAuthorizerParams
{
    public int X { get; set; }
    public int Y { get; set; }
    public string? VaryByValue { get; set; }
    public bool FailAuthorization { get; set; }
}

public class CachedTestQueryPolicy : CQRSOutputCachePolicy<CachedTestQuery>
{
    public override ValueTask CacheRequestCoreAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var query = context.HttpContext.GetCQRSRequestPayload<CachedTestQuery>();
        if (!string.IsNullOrEmpty(query.VaryByValue))
        {
            context.AllowCacheLookup = true;
            context.AllowCacheStorage = true;
            context.CacheVaryByRules.VaryByValues["custom"] = query.VaryByValue;
        }
        return ValueTask.CompletedTask;
    }

    public override ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var result = context.HttpContext.GetCQRSRequiredResultPayload<TestQueryResult>();
        if (result.Sum < 0)
        {
            context.AllowCacheStorage = false;
        }
        return base.ServeResponseAsync(context, cancellation);
    }
}

public class CachedTestOperationPolicy : CQRSOutputCachePolicy<CachedTestOperation>
{
    public override ValueTask CacheRequestCoreAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var operation = context.HttpContext.GetCQRSRequestPayload<CachedTestOperation>();
        if (!string.IsNullOrEmpty(operation.VaryByValue))
        {
            context.AllowCacheLookup = true;
            context.AllowCacheStorage = true;
            context.CacheVaryByRules.VaryByValues["custom"] = operation.VaryByValue;
        }
        return ValueTask.CompletedTask;
    }

    public override ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var result = context.HttpContext.GetCQRSRequiredResultPayload<TestOperationResult>();
        if (result is { Sum: < 0 })
        {
            context.AllowCacheStorage = false;
        }
        return base.ServeResponseAsync(context, cancellation);
    }
}

public class CachedTestQueryHandler : IQueryHandler<CachedTestQuery, TestQueryResult>
{
    public static int InvocationCount { get; private set; }

    public static void Reset()
    {
        InvocationCount = 0;
    }

    public Task<TestQueryResult> ExecuteAsync(HttpContext context, CachedTestQuery query)
    {
        InvocationCount++;
        context.Response.Headers.ETag = $"\"{Guid.NewGuid():n}\"";
        return Task.FromResult(new TestQueryResult { Sum = query.X + query.Y });
    }
}

public class CachedTestOperationHandler : IOperationHandler<CachedTestOperation, TestOperationResult>
{
    public static int InvocationCount { get; private set; }

    public static void Reset()
    {
        InvocationCount = 0;
    }

    public Task<TestOperationResult> ExecuteAsync(HttpContext context, CachedTestOperation operation)
    {
        InvocationCount++;
        context.Response.Headers.ETag = $"\"{Guid.NewGuid():n}\"";
        return Task.FromResult(new TestOperationResult { Sum = operation.X + operation.Y });
    }
}
