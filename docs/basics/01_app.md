# CoreLib-based apps

CoreLib tries to make developing ASP.NET Core-based apps easier. One of the goals is to provide common features out of the box (e.g. logging & config) and unify how the app is being composed. Here, we describe what is being done and how it affects app structure.

## Used libraries

CoreLib extends ASP.NET Core startup procedure and provides a set of opinionated library/service choices:

 1. [Serilog](https://serilog.net/) for logging,
 2. [Autofac](https://autofac.org/) as IoC/DI container,
 3. [Azure Key Vault](https://azure.microsoft.com/en-in/services/key-vault/) for production configuration,
 4. [Seq](https://datalust.co/seq) for development-time log browsing,
 5. `stdout` for production logging. :)

## Unified project structure

We decided to use Autofac as main IoC/DI container but because many external libraries use [ASP.NET Core-style IoC configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection), we decided to extend Autofac's `IModule` with ASP.NET Core's `ConfigureServices`. This resulted in `IAppModule` interface (and `AppModule` base class) that allows to register dependencies using both styles. It is worth noting that **Autofac-style registration is preferred**, the other is only there for compatibility.

The `IAppModule` interface is defined in `LeanCode.Components` project.

### Recommendations

`IAppModule` is the main building block for our modular projects. Each project **should** provide **at most one** `IAppModule` implementation that is the main entry point to that project. Registering project's module should suffice to use the project, provided that it's dependencies are also registered. It is developer's job to register all the necessary dependencies.

If project requires separate, user-provided configuration (e.g. api keys, certificates), don't require it in module. Leave it to app to register it separately.

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

    protected override IAppModule[] Modules { get; }

    public Startup(IConfiguration config, IHostEnvironment hostEnv)
        : base(config)
    {
        this.hostEnv = hostEnv;

        Modules = new IAppModule[]
        {
            new CQRSModule()
                .WithDefaultPipelines<AppContext>(TypesCatalog.Of<Startup>()),
            new SendGridModule(),
            // And many other
        };
    }

    protected override void ConfigureApp(IApplicationBuilder app)
    {
        if (hostEnv.IsDevelopment() || hostEnv.IsStaging())
        {
            app.UseDeveloperExceptionPage();
        }

        app.Map("/api/mobile", api => api
            .UseRemoteCQRS(TypesCatalog.Of<Startup>(), AppContext.FromHttp));
    }
}
```

## Other recommendations

 1. Don't use `appsettings.json` - use environment variables,
 2. Don't use `user secrets` - there are other, more docker-friendly approaches,
 3. Don't structure configuration according to module configs, it will be unmaintainable. Use some kind of mapping.
