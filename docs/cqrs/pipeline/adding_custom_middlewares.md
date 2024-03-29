
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

* `GetCQRSRequestPayload()`: This `HttpContext` extension method, returns [CQRSRequestPayload] which contains information about the request payload and the execution result, if the request was handled.

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
                            .CQRSTrace()
                            .BlockEmployees()
                            .Secure()
                            .Validate()
                            .CommitTransaction<CoreDbContext>()
                            .PublishEvents();

                        cqrs.Queries = c =>
                            c.CQRSTrace()
                            .BlockEmployees()
                            .Secure();

                        cqrs.Operations = c =>
                            c.CQRSTrace()
                            .BlockEmployees()
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
