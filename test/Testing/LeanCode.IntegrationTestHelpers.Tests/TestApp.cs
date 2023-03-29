using System.Collections.Generic;
using System.Reflection;
using LeanCode.Components;
using LeanCode.Components.Startup;
using LeanCode.IntegrationTestHelpers.Tests.App;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LeanCode.IntegrationTestHelpers.Tests;

public class TestApp : LeanCodeTestFactory<Startup>
{
    protected override ConfigurationOverrides Configuration { get; } = new();

    protected override IEnumerable<Assembly> GetTestAssemblies()
    {
        yield return typeof(Startup).Assembly;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSolutionRelativeContentRoot("test/Testing/LeanCode.IntegrationTestHelpers.Tests");
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return LeanProgram
            .BuildMinimalHost<Startup>()
            .ConfigureDefaultLogging(projectName: "integration-tests", destructurers: new TypesCatalog(typeof(Program)))
            .UseEnvironment(Environments.Development);
    }
}
