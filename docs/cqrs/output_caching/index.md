# Output Caching

Output caching is a performance optimization technique that stores the complete HTTP response of CQRS [query] or [operation].
When a subsequent identical request arrives, the cached response is served directly without executing the handler,
reducing database load and improving response times.

LeanCode CoreLibrary integrates ASP.NET Core's built-in [OutputCaching middleware](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output?view=aspnetcore-9.0) into the CQRS pipeline,
placing it at the correct position where cache policies have access to the fully deserialized CQRS request
and response objects. This provides a type-safe way to define caching behavior per query or operation based
on the actual request payload, not just HTTP-level data.

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.CQRS.OutputCaching | [![NuGet version (LeanCode.CQRS.OutputCaching)](https://img.shields.io/nuget/vpre/LeanCode.CQRS.OutputCaching.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.CQRS.OutputCaching) | Output caching policies |

## Why use Output Caching?

- **Performance improvement:** Significantly reduces response times for frequently requested data by serving cached responses.
- **Reduced database load:** Minimizes database queries for cacheable data, allowing your system to handle more concurrent users.
- **Bandwidth savings:** Supports HTTP conditional requests (ETags, Last-Modified) to minimize data transfer.

### Why integrate Output Caching into our CQRS?

- **Type-safe policies:** Define caching behavior per query/operation with full access to **deserialized** request
  and response payloads, not just HTTP-level data.
- **Correct pipeline integration:** The middleware can be placed at the right position in the CQRS pipeline,
  ensuring cache policies have access to the fully processed CQRS request object.

## Limitations

!!! warning "Local Execution Not Supported"
    Output caching **does not work** with [local execution]. The ASP.NET Core OutputCaching middleware requires
    a real HTTP context with stream manipulation capabilities that are not available in the local execution pipeline.
    If you need caching with local execution, consider implementing an application-level cache instead.

!!! info "Commands Not Supported"
    Output caching is only available for **queries** and **operations**.
    Commands change system state and should not be cached.

## Configuration

### Registration

To enable output caching in your application, register it in the `ConfigureServices` method:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    // Register CQRS
    services.AddCQRS(TypesCatalog.Of<ExampleQuery>(), TypesCatalog.Of<ExampleHandler>());

    // Register output caching with automatic policy discovery
    services.AddCQRSOutputCache(TypesCatalog.Of<ExampleHandler>()); // Catalog with the policies
}
```

The `AddCQRSOutputCache` method:

- Automatically discovers all cache policies in the specified assemblies
- Registers them with the DI container
- Configures the ASP.NET Core OutputCache middleware to use them
- Adds endpoint metadata to apply caching to the appropriate CQRS endpoints

!!! warning "Cache Policies Required"
    **Important:** Queries and operations are **cached only if** they have a corresponding cache policy class
    that implements `ICQRSOutputCachePolicy<T>` for their respective type `T`.
    The `AddCQRSOutputCache` method scans the provided assemblies for policy implementations.
    Without a policy, a query/operation will execute normally even if `.CacheOutput()` is in the pipeline.

### Enabling in the CQRS Pipeline

To enable output caching for CQRS endpoints, simply add `.CacheOutput()` to the query and/or operation pipelines:

```csharp
protected override void ConfigureApp(IApplicationBuilder app)
{
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapRemoteCQRS("/api", cqrs =>
        {
            // Enable caching in the query pipeline
            cqrs.Queries = q => q
                .Secure()
                .CacheOutput();  // <- Add this to enable output caching

            // Enable caching in the operation pipeline
            cqrs.Operations = o => o
                .Secure()
                .CacheOutput();  // <- Add this to enable output caching

            // Commands don't support caching
            cqrs.Commands = c => c
                .Secure()
                .Validate();
        });
    });
}
```

!!! danger "Do NOT Use app.UseOutputCache() for CQRS Endpoints"
    You **must not** call `app.UseOutputCache()` globally when using output caching with CQRS endpoints.
    The `.CacheOutput()` method in the CQRS pipeline automatically adds the OutputCache middleware
    at the correct position in the pipeline - **after** the CQRS request payload has been deserialized
    and is available to cache policies. Adding `app.UseOutputCache()` globally would add the middleware
    in the wrong place (before CQRS deserialization), preventing cache policies from accessing the request payload
    and breaking the caching functionality.

!!! tip "CacheOutput() Placement"
    The `.CacheOutput()` method should typically be called after security middlewares.
    This ensures that only authorized requests may get cached responses, so that reduces a class of potential bugs.

!!! tip "Mixed Scenarios: CQRS + Other Endpoints"
    If you need output caching for **both** CQRS endpoints and non-CQRS endpoints (e.g., minimal APIs, controllers), make sure to apply the `UseOutputCache()` middleware **only** to non-CQRS endpoints. You can use eg. `UseWhen(...)` or `Map(...)` for that as shown below:

```csharp
protected override void ConfigureApp(IApplicationBuilder app)
{
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    // Apply output caching ONLY to non-CQRS endpoints
    app.UseWhen(
        ctx => !ctx.Request.Path.StartsWithSegments("/api/cqrs"),
        app => app.UseOutputCache()
    );

    app.UseEndpoints(endpoints =>
    {
        // CQRS endpoints use .CacheOutput() in their pipeline
        endpoints.MapRemoteCQRS("/api/cqrs", cqrs =>
        {
            cqrs.Queries = q => q.Secure().CacheOutput();
            cqrs.Operations = o => o.Secure().CacheOutput();
        });

        // Other endpoints can use .CacheOutput() attribute
        endpoints.MapGet("/api/health", () => "OK").CacheOutput();
    });
}
```

or

```csharp
protected override void ConfigureApp(IApplicationBuilder app)
{
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.Map("/api/cqrs", app => UseEndpoints(endpoints =>
        // CQRS endpoints use .CacheOutput() in their pipeline
        endpoints.MapRemoteCQRS("/", cqrs =>
        {
            cqrs.Queries = q => q.Secure().CacheOutput();
            cqrs.Operations = o => o.Secure().CacheOutput();
        })));

    // Apply output caching ONLY to non-CQRS endpoints
    app.UseOutputCache();
    app.UseEndpoints(endpoints =>
    {
        // Other endpoints can use .CacheOutput() attribute
        endpoints.MapGet("/api/health", () => "OK").CacheOutput();
    });
}
```

!!! important "Cache Policies Are Required"
    Simply adding `.CacheOutput()` to the pipeline is **not enough** to enable caching.
    Each query or operation you want to cache **must** have a corresponding cache policy class that implements `ICQRSOutputCachePolicy<TObject>`.
    Without a policy, the query/operation will execute normally without any caching.
    Queries and operations without policies are not affected by the `.CacheOutput()` middleware and execute as if caching was not enabled.

## Creating Cache Policies

Cache policies define the caching behavior for specific queries or operations
and must be provided if a query or operation should be considered eligible for caching.
A policy must implement `ICQRSOutputCachePolicy<TObject>` to be scanned from assembly,
which also implements the `IOutputCachePolicy` interface from ASP.NET Core.

While implementing the policies it is recommended to actually inherit from the `CQRSOutputCachePolicy<TObject> : ICQRSOutputCachePolicy<TObject>`,
which also sets up some basic caching flags while using the base methods.

During implementing a policy we have access to the to both the request payload and the response payload,
in order to be able to make some decisions basing on them.

### Query Cache Policy

For queries, extend `CQRSQueryOutputCachePolicy<TQuery, TResult>`:

```csharp
using LeanCode.CQRS.OutputCaching.BasePolicies;
using Microsoft.AspNetCore.OutputCaching;

public class GetProjectQuery : IQuery<ProjectDTO>
{
    public string ProjectId { get; set; }
}

public class GetProjectCachePolicy : CQRSQueryOutputCachePolicy<GetProjectQuery, ProjectDTO>
{
    public override ValueTask CacheRequestCoreAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        // Enable caching for this query
        context.AllowCacheLookup = true;
        context.AllowCacheStorage = true;

        // Cache for 5 minutes
        context.ResponseExpirationTimeSpan = TimeSpan.FromMinutes(5);

        // Vary cache by ProjectId
        var query = context.HttpContext.GetCQRSRequestPayload<GetProjectQuery>();;
        context.CacheVaryByRules.VaryByValues["projectId"] = query.ProjectId;

        return ValueTask.CompletedTask;
    }

    public override ValueTask ServeResponseAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        // Optionally inspect the result to decide if it should be cached
        var result = context.HttpContext.GetCQRSRequiredResultPayload<ProjectDTO>();

        if (result is null)
        {
            // Don't cache null results
            context.AllowCacheStorage = false;
        }

        return ValueTask.CompletedTask;
    }
}
```

### Operation Cache Policy

For operations, extend `CQRSOperationOutputCachePolicy<TOperation, TResult>`:

```csharp
public class GenerateProjectReportOperation : IOperation<ReportDTO>
{
    public string ProjectId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public ReportFormat Format { get; set; }  // PDF, CSV, etc.
}

public class GenerateProjectReportCachePolicy
    : CQRSOperationOutputCachePolicy<GenerateProjectReportOperation, ReportDTO>
{
    public override ValueTask CacheRequestCoreAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        var operation = context.HttpContext.GetCQRSRequestPayload<GenerateProjectReportOperation>();

        // Enable caching for completed historical reports
        // (Don't cache if the date range includes today - data is still changing)
        if (operation.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            context.AllowCacheLookup = true;
            context.AllowCacheStorage = true;

            // Cache historical reports for 1 hour (they're expensive to generate)
            context.ResponseExpirationTimeSpan = TimeSpan.FromHours(1);

            // Vary by all operation parameters
            context.CacheVaryByRules.VaryByValues["projectId"] = operation.ProjectId;
            context.CacheVaryByRules.VaryByValues["startDate"] = operation.StartDate.ToString("yyyy-MM-dd");
            context.CacheVaryByRules.VaryByValues["endDate"] = operation.EndDate.ToString("yyyy-MM-dd");
            context.CacheVaryByRules.VaryByValues["format"] = operation.Format.ToString();
        }

        return ValueTask.CompletedTask;
    }

    public override ValueTask ServeResponseAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        var result = context.HttpContext.GetCQRSRequiredResultPayload<ReportDTO>();

        // Don't cache if report generation failed
        if (result?.FileContent == null)
        {
            context.AllowCacheStorage = false;
        }

        return ValueTask.CompletedTask;
    }
}
```

## Policy Lifecycle Methods

Cache policies can override three methods that correspond to different stages of the caching middleware:

### CacheRequestCoreAsync

This is `CacheRequestAsync` from the base interface, but renamed to `CacheRequestCoreAsync` in the `CQRSOutputCachePolicy` base class.

Called **before** checking if a cached response exists. This is the main method where you should implement caching logic like:

- Enable or disable caching for the request
- Configure cache key variation rules
- Set expiration times

```csharp
public override ValueTask CacheRequestCoreAsync(
    OutputCacheContext context,
    CancellationToken cancellationToken)
{
    // Access the request payload
    var query = context.HttpContext.GetCQRSRequestPayload<ExampleQuery>();

    // Enable caching
    context.AllowCacheLookup = true;
    context.AllowCacheStorage = true;

    // Configure cache behavior
    context.ResponseExpirationTimeSpan = TimeSpan.FromMinutes(10);

    // Vary cache by specific properties
    context.CacheVaryByRules.VaryByValues["key"] = query.SomeProperty;

    return ValueTask.CompletedTask;
}
```

### ServeFromCacheAsync

Called when a cached entry is **found** and about to be served. Use this in exceptional cases to:

- Modify response headers before serving from cache

```csharp
public override ValueTask ServeFromCacheAsync(
    OutputCacheContext context,
    CancellationToken cancellationToken)
{
    // You can inspect the cached response
    // and decide whether to serve it

    return ValueTask.CompletedTask;
}
```

### ServeResponseAsync

Called after the handler executes and **before** storing the response in cache. Use this to:

- Inspect the result and conditionally disable storing in cache

```csharp
public override ValueTask ServeResponseAsync(
    OutputCacheContext context,
    CancellationToken cancellationToken)
{
    // Access the response payload
    var result = context.HttpContext.GetCQRSRequiredResultPayload<ExampleResult>();

    // Don't cache null responses
    if (result is null)
    {
        context.AllowCacheStorage = false;
    }

    return ValueTask.CompletedTask;
}
```

## Accessing Request and Response Payloads

Cache policies provide helper methods to access the CQRS object and result:

```csharp
public class ExampleCachePolicy : CQRSQueryOutputCachePolicy<ExampleQuery, ExampleResult>
{
    public override ValueTask CacheRequestAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        context.AllowCacheLookup = true;
        context.AllowCacheStorage = true;

        // Get the query/operation object
        var query = context.HttpContext.GetCQRSRequestPayload<ExampleQuery>();

        // Use query properties to configure caching
        context.CacheVaryByRules.VaryByValues["id"] = query.Id;

        return ValueTask.CompletedTask;
    }

    public override ValueTask ServeResponseAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        // Get the result object (only available after handler execution)
        var result = context.HttpContext.GetCQRSRequiredResultPayload<ExampleResult>();

        // Conditionally disable storing in cache basing on result
        if (result is null)
        {
            context.AllowCacheStorage = false;
        }

        return ValueTask.CompletedTask;
    }
}
```

## Cache Variation

Cache variation determines how cache keys are constructed. Different cache keys store separate cached responses.

### Vary by Query/Operation Properties

```csharp
public override ValueTask CacheRequestAsync(
    OutputCacheContext context,
    CancellationToken cancellationToken)
{
    var query = context.HttpContext.GetCQRSRequestPayload<ExampleQuery>();

    // Each unique combination creates a separate cache entry
    context.CacheVaryByRules.VaryByValues["userId"] = query.UserId;
    context.CacheVaryByRules.VaryByValues["date"] = query.Date.ToString("yyyy-MM-dd");

    return ValueTask.CompletedTask;
}
```

### Vary by User

To cache different responses per user:

```csharp
public override ValueTask CacheRequestAsync(
    OutputCacheContext context,
    CancellationToken cancellationToken)
{
    // Vary by authenticated user
    if (context.HttpContext.User.Identity?.IsAuthenticated == true)
    {
        var userId = context.HttpContext.User.FindFirst("sub")?.Value;
        context.CacheVaryByRules.VaryByValues["user"] = userId ?? "";
    }

    return ValueTask.CompletedTask;
}
```

### Vary by Headers

```csharp
public override ValueTask CacheRequestAsync(
    OutputCacheContext context,
    CancellationToken cancellationToken)
{
    // Vary by Accept-Language header
    context.CacheVaryByRules.HeaderNames.Add("Accept-Language");

    return ValueTask.CompletedTask;
}
```

## ETags and Conditional Requests

ASP.NET Core's output caching automatically handles ETags when you set them in your handler:

```csharp
public class GetProjectHandler : IQueryHandler<GetProjectQuery, ProjectDTO>
{
    public async Task<ProjectDTO> ExecuteAsync(HttpContext context, GetProjectQuery query)
    {
        var project = await GetProjectAsync(query.ProjectId);

        // Set ETag based on project version
        context.Response.Headers.ETag = $"\"{project.Version}\"";

        return project;
    }
}
```

When the middleware caches this response, it stores the ETag. On subsequent requests with `If-None-Match` headers, it automatically returns `304 Not Modified` if the ETag matches.

## Cache Eviction

Look up the official [ASP.NET Core documentation](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output?view=aspnetcore-10.0#use-tags-to-evict-cache-entries) for details on cache eviction.

In order to set up tags for cache eviction set them in your cache policy:

```csharp
public override ValueTask CacheRequestCoreAsync(
    OutputCacheContext context,
    CancellationToken cancellationToken)
{
    // Enable caching
    context.AllowCacheLookup = true;
    context.AllowCacheStorage = true;
    // Set tags for eviction
    context.Tags.Add("projects");
    return ValueTask.CompletedTask;
}
```

## Advanced Configuration

### Custom Cache Store

By default, output caching uses an in-memory store. For distributed scenarios, configure a custom cache store. Note that `AddCQRSOutputCache` automatically registers the output cache service (via `services.AddOutputCache()`), so you can configure it directly:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    // Register CQRS
    services.AddCQRS(TypesCatalog.Of<ExampleQuery>(), TypesCatalog.Of<ExampleHandler>());

    // Register CQRS output caching
    services.AddCQRSOutputCache(TypesCatalog.Of<ExampleHandler>());

    // Configure output cache to use Redis for distributed caching
    services.AddStackExchangeRedisOutputCache(options =>
    {
        options.Configuration = "localhost:6379";
    });
}
```

### Global Cache Settings

Configure default settings for output caching providing `Action<OutputCacheOptions>` in `AddCQRSOutputCache`:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    services.AddCQRSOutputCache(
        TypesCatalog.Of<ExampleHandler>(),
        options =>
        {
            options.DefaultExpirationTimeSpan = TimeSpan.FromMinutes(10);
            options.MaximumBodySize = 1024 * 1024; // 1MB
            options.SizeLimit = 100 * 1024 * 1024; // 100MB total
        });
}
```

## Best Practices

### ✅ Do

- **Cache queries with stable data:** Queries that return data that doesn't change frequently are ideal candidates for caching.
- **Cache expensive operations with deterministic outputs:** Operations like report generation, file exports, or data transformations that are expensive to compute but produce the same result for the same inputs are excellent candidates for caching.
- **Set appropriate expiration times:** Balance freshness with performance based on your data's volatility and computational cost.
- **Vary by relevant properties:** Ensure cache keys capture all factors that affect the response.
- **Consider memory usage:** Monitor cache size and set appropriate limits, especially for operations that return large files.
- **Use ETags for large responses:** Reduce bandwidth usage with conditional requests, particularly important for generated files.
- **Test cache behavior:** Verify that cache invalidation and variation work as expected.

### ❌ Don't

- **Don't use `app.UseOutputCache()` globally:** Never add this middleware to the main pipeline when you have CQRS endpoints. Use `.CacheOutput()` in the CQRS pipeline instead. If you need caching for non-CQRS endpoints, use `app.UseWhen()` to apply it conditionally.
- **Don't cache commands:** Commands change state and caching them is not supported.
- **Don't cache operations with side effects that must execute every time:** If an operation has important side effects (e.g., logging, analytics, notifications) that need to happen on every call, it should not be cached. Only cache operations where the primary value is the returned data.
- **Don't cache user-specific data without varying by user:** Always vary by user ID for personalized data.
- **Don't set very long expiration times:** Stale data can cause confusion and bugs.
- **Don't assume local execution with output cache works:** Output caching only works with HTTP requests, local executor will fail if output caching is attempted on the local pipeline.

## Testing Cache Policies

When testing queries and operations with cache policies:

```csharp
[Fact]
public async Task Query_is_cached_correctly()
{
    var client = factory.CreateClient();
    var request = new GetProjectQuery { ProjectId = "123" };

    // First request - executes handler
    var response1 = await client.PostAsJsonAsync("/api/query/GetProjectQuery", request);
    response1.EnsureSuccessStatusCode();

    // Second identical request - served from cache
    var response2 = await client.PostAsJsonAsync("/api/query/GetProjectQuery", request);
    response2.EnsureSuccessStatusCode();

    // Check Age header indicates cached response
    response2.Headers.Should().ContainKey("Age");
}
```

## Additional Resources

- [ASP.NET Core Output Caching Documentation](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output)
- [CoreLibrary Pipeline Documentation](../pipeline/index.md)

[local execution]: ../local_execution/index.md
[query]: ../query/index.md
[operation]: ../operation/index.md
