using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.OutputCaching.BasePolicies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;

namespace LeanCode.CQRS.AspNetCore.Tests.Integration.OutputCache;

public class CachedTestQuery : IQuery<TestQueryResult>
{
    public int X { get; set; }
    public int Y { get; set; }
    public string? VaryByValue { get; set; }
}

public class CachedTestOperation : IOperation<TestOperationResult>
{
    public int X { get; set; }
    public int Y { get; set; }
    public string? VaryByValue { get; set; }
}

public class CachedTestQueryPolicy : CQRSQueryOutputCachePolicy<CachedTestQuery, TestQueryResult>
{
    public override async ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        await base.CacheRequestAsync(context, cancellationToken);

        var query = GetRequestPayload(context.HttpContext);
        if (!string.IsNullOrEmpty(query.VaryByValue))
        {
            context.AllowCacheLookup = true;
            context.AllowCacheStorage = true;
            context.CacheVaryByRules.VaryByValues["custom"] = query.VaryByValue;
        }
    }

    public override ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        var result = GetResultPayload(context.HttpContext);
        if (result is { Sum: < 0 })
        {
            context.AllowCacheStorage = false;
        }
        return base.ServeResponseAsync(context, cancellationToken);
    }
}

public class CachedTestOperationPolicy : CQRSOperationOutputCachePolicy<CachedTestOperation, TestOperationResult>
{
    public override async ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        await base.CacheRequestAsync(context, cancellationToken);

        var operation = GetRequestPayload(context.HttpContext);
        if (!string.IsNullOrEmpty(operation.VaryByValue))
        {
            context.AllowCacheLookup = true;
            context.AllowCacheStorage = true;
            context.CacheVaryByRules.VaryByValues["custom"] = operation.VaryByValue;
        }
    }

    public override ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        var result = GetResultPayload(context.HttpContext);
        if (result is { Sum: < 0 })
        {
            context.AllowCacheStorage = false;
        }
        return base.ServeResponseAsync(context, cancellationToken);
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
