
# Adding custom middlewares

Custom middlewares can further augment the pipeline by employing the `UseMiddleware` method on `IApplicationBuilder`. To illustrate, consider the following basic example, which introduces a check if user is blocked into the pipeline:

```csharp
    public class UserBlockerMiddleware : IMiddleware
    {
        private readonly UserBlocker userBlocker;

        public EnsureCallerIsSyncedMiddleware(UserBlocker userBlocker)
        {
            this.userBlocker = userBlocker;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (userBlocker.IsUserBlocked(context.UserId()))
            {
                throw new UnauthenticatedException("User blocked.");
            }
            else
            {
                await next(context);
            }
        }
    }

    public static class UserBlockerMiddlewareExtensions
    {
        public static IApplicationBuilder BlockUsers(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserBlockerMiddleware>();
        }
    }
```

Then, you can integrate `UserBlockerMiddleware` into the pipeline as follows:

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
                            .BlockUsers()
                            .Secure()
                            .Validate()
                            .CommitTransaction<CoreDbContext>()
                            .PublishEvents();

                        cqrs.Queries = c =>
                            c.CQRSTrace()
                            .BlockUsers()
                            .Secure();

                        cqrs.Operations = c =>
                            c.CQRSTrace()
                            .BlockUsers()
                            .Secure()
                            .CommitTransaction<CoreDbContext>()
                            .PublishEvents();
                    }
                );
            });
    }
```
