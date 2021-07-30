using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using IdentityModel.Client;
using LeanCode.Components;
using LeanCode.Components.Startup;
using LeanCode.CQRS.RemoteHttp.Client;
using LeanCode.IntegrationTestHelpers;
using LeanCode.IntegrationTests.App;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LeanCode.IntegrationTests
{
    public class TestApp : LeanCodeTestFactory<Startup>
    {
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
        }

        protected override IEnumerable<Assembly> GetTestAssemblies()
        {
            yield return typeof(Startup).Assembly;
        }

        public Task<bool> AuthenticateAsync()
        {
            return AuthenticateAsync(new PasswordTokenRequest
            {
                UserName = AuthConfig.Username,
                Password = AuthConfig.Password,
                Scope = "profile openid api",

                ClientId = "web",
                ClientSecret = "",
            });
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            return LeanProgram
                .BuildMinimalHost<Startup>()
                .ConfigureDefaultLogging(
                    projectName: "test",
                    destructurers: new TypesCatalog(typeof(Program)))
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

        public override async Task DisposeAsync()
        {
            Command = null!;
            Query = null!;
            await base.DisposeAsync();
        }
    }
}
