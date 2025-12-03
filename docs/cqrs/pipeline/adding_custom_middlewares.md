
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

* `GetCQRSObjectMetadata()`: This method returns [CQRSObjectMetadata] which provides access to metadata about the endpoint, providing details such as object types, result types, and handler types involved in the request.

* `GetCQRSRequestPayload()`: This `HttpContext` extension method returns [CQRSRequestPayload], which contains information about the request payload. A generic version `GetCQRSRequestPayload<TPayload>()` is also available to get the strongly-typed payload directly.

* `GetCQRSExecutionResult()`: This `HttpContext` extension method returns the [ExecutionResult] (nullable), if it was produced by the CQRS pipeline after processing the request.

* `GetCQRSRequiredExecutionResult()`: Similar to `GetCQRSExecutionResult()`, but returns a non-nullable [ExecutionResult] and throws if not present.

* `GetCQRSResultPayload<TPayload>()`: Returns the strongly-typed result payload if available, or `default` if the execution result is not present or has no payload.

* `GetCQRSRequiredResultPayload<TPayload>()`: Returns the strongly-typed result payload, throwing an exception if the execution result or payload is not present.

* `SetCQRSExecutionResult(ExecutionResult result)`: This method sets the [ExecutionResult] in the HttpContext features. Use this when your middleware needs to short-circuit the pipeline and return a result. The response will be automatically serialized at the pipeline boundary (by [CQRSMiddleware]).

!!! tip "Short-Circuiting Middlewares"
    If your custom middleware short-circuits the pipeline (i.e., doesn't call `await next(context)`), you have two options: **set the `ExecutionResult`** by calling `context.SetCQRSExecutionResult(result)` and then `return` (serialization is handled automatically by [CQRSMiddleware]), or **write the response directly** to `context.Response` if you need more control (e.g., streaming). In the latter case, you do not have to set the `ExecutionResult` - the [CQRSMiddleware] will skip serialization if the response has already started.

Here's an example of a middleware that short-circuits and returns a cached result:

```csharp
public class CustomCachedResultMiddleware : IMiddleware
{
    private readonly IResultCache cache;

    public CustomCachedResultMiddleware(IResultCache cache)
    {
        this.cache = cache;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var payload = context.GetCQRSRequestPayload();
        var cacheKey = GenerateCacheKey(payload.Payload);

        if (cache.TryGet(cacheKey, out var cachedResult))
        {
            // Short-circuit: set result and return without calling next()
            // Serialization is handled automatically by CQRSMiddleware
            context.SetCQRSExecutionResult(ExecutionResult.WithPayload(cachedResult));
            return;
        }

        // Continue pipeline
        await next(context);

        // Cache the result after execution
        var result = context.GetCQRSExecutionResult();
        if (result?.StatusCode == 200)
        {
            cache.Set(cacheKey, result.Value.Payload);
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
[CQRSMiddleware]: https://github.com/leancodepl/corelibrary/blob/HEAD/src/CQRS/LeanCode.CQRS.AspNetCore/Middleware/CQRSMiddleware.cs
