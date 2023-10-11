
# Adding custom middlewares

Custom middlewares can further augment the pipeline by employing the `UseMiddleware` method on `IApplicationBuilder`. To illustrate, consider the following basic example, which introduces a check if employee is blocked into the pipeline:

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

Then, you can integrate `EmployeeBlockerMiddleware` into the pipeline as follows:

```csharp
    protected override void ConfigureApp(IApplicationBuilder app)
    {
        . . .
        app.UseEndpoints(endpoints =>
            {
                endpoints.MapRemoteCqrs(
                    "/api",
                    cqrs =>
                    {
                        cqrs.Commands = c =>
                            c.CQRSTrace()
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
