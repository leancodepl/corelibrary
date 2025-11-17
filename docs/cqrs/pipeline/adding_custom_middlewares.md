
# Adding custom middlewares

Custom middlewares can further augment the pipeline by employing the `UseMiddleware` method on `IApplicationBuilder`. To illustrate, consider the following basic example, which introduces a check if employee is blocked into the pipeline using [ASP.NET middleware]:

```csharp
    public class EmployeeBlockerMiddleware : IMiddleware
    {
        private readonly EmployeeBlocker employeeBlocker;

        public EmployeeBlockerMiddleware(EmployeeBlocker employeeBlocker)
        {
            this.employeeBlocker = employeeBlocker;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (employeeBlocker.IsEmployeeBlocked(context.EmployeeId()))
            {
                throw new UnauthenticatedException("Employee blocked.");
            }
            else
            {
                await next(context);
            }
        }
    }

    public static class EmployeeBlockerMiddlewareExtensions
    {
        public static IApplicationBuilder BlockEmployees(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<EmployeeBlockerMiddleware>();
        }
    }
```

Moreover, CoreLibrary provides extension methods for `HttpContext`, which can be useful when creating middlewares:

* `GetCQRSEndpoint()`: This method returns [CQRSObjectMetadata] which provides access to metadata about the endpoint, providing details such as object types, result types, and handler types involved in the request.

* `GetCQRSRequestPayload()`: This `HttpContext` extension method returns [CQRSRequestPayload], which contains information about the request payload.

* `GetCQRSExecutionResult()`: This `HttpContext` extension method returns the [ExecutionResult], if it was produced by the CQRS pipeline after processing the request.

* `CompleteCQRSExecutionResult()`: This method should be called when a custom middleware short-circuits the pipeline and needs to return a result payload. It properly sets the [ExecutionResult] and serializes it to the response body. If your middleware returns a response without calling the next middleware, use this method to ensure the response is correctly formatted.

!!! warning "Short-Circuiting Middlewares"
    If your custom middleware short-circuits the pipeline (i.e., doesn't call `await next(context)`) and needs to return a CQRS result payload, you **must** call `await context.CompleteCQRSExecutionResult(result)` ([CompleteCQRSExecutionResult]) to properly serialize the response. Simply setting the response manually will not work correctly with the CQRS pipeline in local execution.

Here's an example of a middleware that short-circuits and returns a cached result:

```csharp
public class CustomCachedResultMiddleware : IMiddleware
{
    private readonly IResultCache cache;

    public CachedResultMiddleware(IResultCache cache)
    {
        this.cache = cache;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var payload = context.GetCQRSRequestPayload();
        var cacheKey = GenerateCacheKey(payload.Payload);

        if (cache.TryGet(cacheKey, out var cachedResult))
        {
            // Short-circuit: return cached result without calling next()
            await context.CompleteCQRSExecutionResult(
                new ExecutionResult(200, cachedResult, hasPayload: true)
            );
            return;
        }

        // Continue pipeline
        await next(context);

        // Cache the result after execution
        var result = context.GetCQRSRequestPayload().Result;
        if (result?.StatusCode == 200)
        {
            cache.Set(cacheKey, result.Payload);
        }
    }
}
```

After configuration above, you can integrate `EmployeeBlockerMiddleware` into the pipeline as follows:

```csharp
    protected override void ConfigureApp(IApplicationBuilder app)
    {
        // . . .
        app.UseEndpoints(endpoints =>
            {
                endpoints.MapRemoteCQRS(
                    "/api",
                    cqrs =>
                    {
                        cqrs.Commands = c => c
                            .BlockEmployees()
                            .Secure()
                            .Validate()
                            .CommitTransaction<CoreDbContext>()
                            .PublishEvents();

                        cqrs.Queries = c =>
                            c.BlockEmployees()
                            .Secure();

                        cqrs.Operations = c =>
                            c.BlockEmployees()
                            .Secure()
                            .CommitTransaction<CoreDbContext>()
                            .PublishEvents();
                    }
                );
            });
    }
```

[ASP.NET middleware]: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/
[CQRSObjectMetadata]: https://github.com/leancodepl/corelibrary/blob/HEAD/src/CQRS/LeanCode.CQRS.Execution/CQRSObjectMetadata.cs
[CQRSRequestPayload]: https://github.com/leancodepl/corelibrary/blob/HEAD/src/CQRS/LeanCode.CQRS.Execution/CQRSRequestPayload.cs
[ExecutionResult]: https://github.com/leancodepl/corelibrary/blob/HEAD/src/CQRS/LeanCode.CQRS.Execution/ExecutionResult.cs
[CompleteCQRSExecutionResult]: https://github.com/leancodepl/corelibrary/blob/HEAD/src/CQRS/LeanCode.CQRS.AspNetCore/HttpContextExtensions.cs#L13
