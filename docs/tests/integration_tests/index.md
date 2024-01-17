# Integration tests

Integration testing is a critical phase in software development, and LeanCode CoreLibrary significantly streamlines this process by offering support for various testing functionalities. It aids integration testing by creating and deleting databases per test, ensuring a clean and isolated environment for each test scenario and facilitates the execution of command, queries and operations.

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.CQRS.RemoteHttp.Client | [![NuGet version (LeanCode.CQRS.RemoteHttp.Client")](https://img.shields.io/nuget/vpre/LeanCode.CQRS.RemoteHttp.Client.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.CQRS.RemoteHttp.Client) | CQRS client |
| LeanCode.IntegrationTestHelpers | [![NuGet version (LeanCode.IntegrationTestHelpers)](https://img.shields.io/nuget/vpre/LeanCode.IntegrationTestHelpers.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.IntegrationTestHelpers) | Integration tests helper methods |
| LeanCode.Logging | [![NuGet version (LeanCode.Logging)](https://img.shields.io/nuget/vpre/LeanCode.Logging.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.Logging) | Logging |

## Configuration

The provided code demonstrates integration test setup in a sample application. It covers database handling per test (PostgreSQL and SQLServer databases are supported), test authentication, and debugger waiting through the `WAIT_FOR_DEBUGGER environment` variable. The code configures the test environment, sets up authentication, and ensures a clean test scenario for an example application.

```csharp
public class ExampleAppTestApp : LeanCodeTestFactory<Startup>
{
    public readonly Guid SuperAdminId =
        Guid.Parse("4d3b45e6-a2c1-4d6a-9e23-94e0d9f8ca01");

    // Ensure that you add these variables to your environment
    // variables before executing the tests.
    protected override ConfigurationOverrides Configuration { get; } =
        new(connectionStringBase: "Database__ConnectionStringBase",
            connectionStringKey: "Database:ConnectionString");

    static ExampleAppTestApp()
    {
        if (!string.IsNullOrWhiteSpace(
            Environment.GetEnvironmentVariable("WAIT_FOR_DEBUGGER")))
        {
            Console.WriteLine("Waiting for debugger to be attached...");

            while (!Debugger.IsAttached)
            {
                Thread.Sleep(100);
            }
        }
    }

    protected override IEnumerable<Assembly> GetTestAssemblies()
    {
        yield return typeof(ExampleAppTestApp).Assembly;
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        // The `BuildMinimalHost` method includes environment
        // variables as part of the configuration and configures
        // Kestrel as the web server. Ensure that you pass your
        // API's `Startup` class for proper functionality.
        return LeanProgram
            .BuildMinimalHost<Startup>()
            .ConfigureDefaultLogging(
                "ExampleApp-tests",
                new[] { typeof(Program).Assembly })
            .UseEnvironment(Environments.Development);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            // Incorporate a hosted service responsible for creating
            // and dropping the database after each test execution.
            services.AddHostedService<DbContextInitializer<CoreDbContext>>();

            // `AddTestAuthenticationHandler` is an extension method from
            // the `TestAuthenticationHandler` defined further below.
            services.AddAuthentication(TestAuthenticationHandler.SchemeName)
                .AddTestAuthenticationHandler();
        });
    }
}
```

Following `ItemGroup` must be added to the integration test project's `.csproj` file. This group facilitates the discovery of the content root for the web application being tested.

```xml
  <ItemGroup>
    <WebApplicationFactoryContentRootAttribute
      Include="ExampleApp.IntegrationTests"
      AssemblyName="ExampleApp.IntegrationTests"
      ContentRootPath="$(MSBuildProjectDirectory)"
      ContentRootTest="ExampleApp.IntegrationTests.csproj"
      Priority="-1" />
  </ItemGroup>
```

## Authentication handler

The `TestAuthenticationHandler` manages user authentication and deserialization of `ClaimsPrincipal` objects for testing purposes.

```csharp
public class TestAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
    )
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var base64Principal = TryGetBase64Principal();
        if (base64Principal is null)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        try
        {
            var principal = DeserializePrincipal(base64Principal);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception e)
        {
            return Task.FromResult(AuthenticateResult.Fail(e));
        }
    }

    private string? TryGetBase64Principal()
    {
        var rawAuth = Request.Headers.Authorization;
        _ = AuthenticationHeaderValue.TryParse(rawAuth, out var auth);

        return auth?.Scheme == Scheme.Name ? auth.Parameter : null;
    }

    public static string SerializePrincipal(ClaimsPrincipal principal)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        principal.WriteTo(writer);
        return Convert.ToBase64String(ms.ToArray());
    }

    public static ClaimsPrincipal DeserializePrincipal(string base64)
    {
        using var ms = new MemoryStream(Convert.FromBase64String(base64));
        using var reader = new BinaryReader(ms);

        return new ClaimsPrincipal(reader);
    }
}

public static class TestAuthenticationHandlerExtensions
{
    public static AuthenticationBuilder AddTestAuthenticationHandler(
        this AuthenticationBuilder builder,
        Action<AuthenticationSchemeOptions>? config = null)
    {
        return builder
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                TestAuthenticationHandler.SchemeName,
                config
        );
    }
}
```

## Authenticated/Unauthenticated test applications

The `AuthenticatedExampleAppTestApp` and `UnauthenticatedExampleAppTestApp` classes extend the base test application for authenticated and unauthenticated scenarios, respectively. They initialize HTTP executors for queries, commands, and operations with or without authentication.

```csharp
public class AuthenticatedExampleAppTestApp : ExampleAppTestApp
{
    private ClaimsPrincipal claimsPrincipal = new();

    public HttpQueriesExecutor Query { get; private set; } = default!;
    public HttpCommandsExecutor Command { get; private set; } = default!;
    public HttpOperationsExecutor Operation { get; private set; } = default!;

    public AuthenticatedExampleAppTestApp() { }

    public override async Task InitializeAsync()
    {
        AuthenticateAsTestSuperUser();

        void ConfigureClient(HttpClient hc) =>
        {
            hc.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    TestAuthenticationHandler.SchemeName,
                    TestAuthenticationHandler.SerializePrincipal(
                        claimsPrincipal));
        };

        await base.InitializeAsync();

        Query = CreateQueriesExecutor(ConfigureClient);
        Command = CreateCommandsExecutor(ConfigureClient);
        Operation = CreateOperationsExecutor(ConfigureClient);

        await WaitForBusAsync();
    }

    public void AuthenticateAsTestSuperUser()
    {
        claimsPrincipal = new(
            new ClaimsIdentity(
                new Claim[]
                {
                    new("sub", SuperAdminId.ToString()),
                    new("role", "user"),
                    new("role", "admin"),
                },
                authenticationType: TestAuthenticationHandler.SchemeName,
                nameType: "sub",
                roleType: "role"
            )
        );
    }

    public override async ValueTask DisposeAsync()
    {
        Command = default!;
        Query = default!;
        Operation = default!;
        await base.DisposeAsync();
    }
}
```

```csharp
public class UnauthenticatedExampleAppTestApp : ExampleAppTestApp
{
    public HttpQueriesExecutor Query { get; private set; } = default!;
    public HttpCommandsExecutor Command { get; private set; } = default!;
    public HttpOperationsExecutor Operation { get; private set; } = default!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        Query = CreateQueriesExecutor();
        Command = CreateCommandsExecutor();
        Operation = CreateOperationsExecutor();

        await WaitForBusAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        Command = default!;
        Query = default!;
        Operation = default!;
        await base.DisposeAsync();
    }
}
```

## Integration test

The provided test case showcases the creation of a new project using the `CreateProject` command in an authenticated environment. It then verifies the project's creation and checks its details.

To run this test, ensure proper environment variable setup and execute the `dotnet test` command after setting `Database__ConnectionStringBase` and `Database__ConnectionString` environment variables to your PostgreSQL/SQLServer connection string.

```csharp
public class Tests : IAsyncLifetime
{
    private readonly AuthenticatedExampleAppTestApp app;

    public Tests()
    {
        app = new AuthenticatedExampleAppTestApp();
    }

    [Fact]
    public async Task Project_is_correctly_created()
    {
        await app.Command.RunSuccessAsync(
            new CreateProject
            {
                Name = "Project"
            });

        var projects = await app.Query.GetAsync(new AllProjects());
        var project = Assert.Single(projects);

        Assert.Equal("Project", project.Name);
        Assert.Matches("^project_[0-7][0-9A-HJKMNP-TV-Z]{25}$", project.Id);
    }

    public Task InitializeAsync() => app.InitializeAsync();

    public Task DisposeAsync() => app.DisposeAsync().AsTask();
}

public static class ApiClientHelpers
{
    public static async Task RunSuccessAsync(
        this HttpCommandsExecutor executor,
        ICommand command)
    {
        var result = await executor.RunAsync(command);
        result.ValidationErrors.Should().BeEmpty(
            "command {0} needs to pass validation",
            command.GetType().Name);
    }
}
```
