using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using LeanCode.Components;
using LeanCode.Components.Startup;
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

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureServices(services =>
            {
                services.AddTransient<DbContext>(sp => sp.GetService<TestDbContext>());
            });
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return LeanProgram
                .BuildMinimalWebHost<Startup>()
                .UseKestrel()
                .ConfigureDefaultLogging(
                    projectName: "integration-tests",
                    destructurers: new TypesCatalog(typeof(Program)))
                .UseEnvironment(Environments.Development);
        }
    }
}
