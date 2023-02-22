using System.Diagnostics;
using System.Reflection;
using IdentityModel.Client;
using LeanCode.Components;
using LeanCode.Components.Startup;
using LeanCode.CQRS.RemoteHttp.Client;
using LeanCode.IntegrationTestHelpers;
using LeanCode.IntegrationTests.App;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace LeanCode.IntegrationTests;

public class TestApp : LeanCodeTestFactory<Startup>
{
    static TestApp()
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WAIT_FOR_DEBUGGER")))
        {
            Console.WriteLine("Waiting for debugger to be attached...");

            while (!Debugger.IsAttached)
            {
                Thread.Sleep(100);
            }

            Console.WriteLine("Debugger attached");
        }
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
    }

    protected override IEnumerable<Assembly> GetTestAssemblies()
    {
        yield return typeof(Startup).Assembly;
    }

    public async Task<bool> AuthenticateAsync()
    {
        using var request = new PasswordTokenRequest
        {
            UserName = AuthConfig.Username,
            Password = AuthConfig.Password,
            Scope = "profile openid api",
            ClientId = "web",
            ClientSecret = "",
        };
        return await AuthenticateAsync(request);
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return LeanProgram
            .BuildMinimalHost<Startup>()
            .ConfigureDefaultLogging(projectName: "test", destructurers: new TypesCatalog(typeof(Program)))
            .UseEnvironment(Environments.Development);
    }
}

public class AuthenticatedTestApp : TestApp
{
    public HttpQueriesExecutor Query { get; private set; } = null!;
    public HttpCommandsExecutor Command { get; private set; } = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        if (!await AuthenticateAsync())
        {
            throw new Xunit.Sdk.XunitException("Authentication failed.");
        }

        Query = CreateQueriesExecutor();
        Command = CreateCommandsExecutor();
    }

    public override ValueTask DisposeAsync()
    {
        Command = null!;
        Query = null!;
        return base.DisposeAsync();
    }
}
