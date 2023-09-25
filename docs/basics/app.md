# CoreLib-based apps

CoreLib tries to make developing ASP.NET Core-based apps easier. One of the goals is to provide common features out of the box (e.g. logging & config) and unify how the app is being composed. Here, we describe what is being done and how it affects app structure.

## Used libraries

CoreLib extends ASP.NET Core startup procedure and provides a set of opinionated library/service choices:

1. [Serilog](https://serilog.net/) for logging,
2. [Azure Key Vault](https://azure.microsoft.com/en-in/services/key-vault/) for production configuration,
3. [Seq](https://datalust.co/seq) for development-time log browsing,
4. [OpenTelemetry-compatible](https://opentelemetry.io/) tooling for metrics & tracing,
5. `stdout` for production logging. :)

## Unified project structure

CoreLibrary tries to adhere to the standards that ASP.NET Core dictates. This means, that it follows the [ASP.NET Core-style IoC configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection), i.e. each module has an extension method that can be used to register all the necessary dependencies.

Projects are free to select their DI container and we have a separate set of projects to aid in using [Autofac](https://autofac.org/):

1. [LeanCode.Components.Autofac] that has the `IAppModule` that bridges using Microsoft DI and Autofac together,
2. [LeanCode.Startup.Autofac] that simplifies Autofac-based app bootstrap.

### A note on CoreLib v7

Previously, CoreLib-based projects must have used Autofac as DI container. CoreLib provided a set of `IModule`s that could be composed together to create the whole app. We don't use that now and you can find a detailed migration guide [here](../guides/advanced/v8_migration_guide.md).

### Recommendations

When building apps, each project should have at most one DI extension method that is the entry point to that project.  Registering project's DI services should suffice to use the project, provided that it's dependencies are also registered. It is developer's job to register all the necessary dependencies.

If project requires separate, user-provided configuration (e.g. api keys, certificates), take it in the configuration method directly. This ensures that everything will be wired-up correctly, without random runtime errors. You can find [an example here].

## Startup

ASP.NET Core startup is quite involved and provides multiple injection points where different things can be configured. To unify the startup procedure we decided to provide a set of classes that simplify the process:

 1. `LeanProgram`/`IHostBuilderExtensions` - a helper classes to build and configure `IHostBuilder`,
 2. `LeanStartup` - extension to ASP.NET Core `Startup` that provides sensible defaults and integrates with the module-based approach to project structure.

Using these classes is sufficient to run ASP.NET Core app with correct configuration.

### Example

```csharp
public class Program
{
    public static void Main()
    {
        BuildHost().Run();
    }

    public static IHost BuildHost()
    {
        return LeanProgram.BuildMinimalHost<Startup>()
            .AddAppConfigurationFromAzureKeyVault()
            .ConfigureDefaultLogging(
                projectName: "test-project",
                appName: "test-project-api",
                destructurers: TypesCatalog.Of<Program>())
            .Build();
    }
}

public class Startup : LeanStartup
{
    private readonly IHostEnvironment hostEnv;

    public Startup(IConfiguration config, IHostEnvironment hostEnv)
        : base(config)
    {
        this.hostEnv = hostEnv;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddCQRS(TypesCatalog.Of<Startup>());
        services.AddFluentValidation(TypesCatalog.Of<Startup>());

        services.AddCoreServices("Example Connection String");
        services.AddApiServices(Configuration, hostEnv);
    }

    protected override void ConfigureApp(IApplicationBuilder app)
    {
        app.UseAuthentication()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/live/health");

                endpoints.MapRemoteCqrs(
                    "/api",
                    cqrs =>
                    {
                        cqrs.Commands = c =>
                            c.CQRSTrace().Secure().Validate().CommitTransaction<CoreDbContext>().PublishEvents();
                        cqrs.Queries = c => c.CQRSTrace().Secure();
                        cqrs.Operations = c =>
                            c.CQRSTrace().Secure().CommitTransaction<CoreDbContext>().PublishEvents();
                    }
                );
            });
    }
}
```

## Other recommendations

1. Don't use `appsettings.json` - use environment variables,
2. Don't use `user secrets` - there are other, more docker-friendly approaches,
3. Don't structure configuration according to module configs, it will be unmaintainable. Use some kind of mapping.
4. If you are using `LeanStartup` in integration tests you may want to set `CloseAndFlushLogger` property to false, because by default `LeanStartup` forces `Log.CloseAndFlush` when the application stops.

[LeanCode.Components.Autofac]: https://github.com/leancodepl/corelibrary/tree/v8.0-preview/src/Core/LeanCode.Components.Autofac
[LeanCode.Startup.Autofac]: https://github.com/leancodepl/corelibrary/tree/v8.0-preview/src/Core/LeanCode.Startup.Autofac
[an example here]: https://github.com/leancodepl/corelibrary/blob/v8.0-preview/src/Infrastructure/LeanCode.SendGrid/SendGridServiceCollectionExtensions.cs#L9
-L15
