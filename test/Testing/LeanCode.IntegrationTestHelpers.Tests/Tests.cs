using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeanCode.CQRS.RemoteHttp.Client;
using LeanCode.IntegrationTestHelpers.Tests.App;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace LeanCode.IntegrationTestHelpers.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1001", Justification = "Disposed with `IAsyncLifetime`.")]
public class Tests : IAsyncLifetime
{
    private readonly TestApp app;

    private HttpQueriesExecutor query = null!;
    private HttpCommandsExecutor command = null!;

    public Tests()
    {
        app = new TestApp();
    }

    [Fact]
    public void Test_services_order_is_correct()
    {
        var hostedServices = app.Services.GetRequiredService<IEnumerable<IHostedService>>();
        Assert.IsType<ConnectionKeeper>(hostedServices.FirstOrDefault());
        Assert.IsType<DbContextInitializer<TestDbContext>>(hostedServices.Skip(1).FirstOrDefault());
    }

    [Fact]
    public async Task Save_and_load()
    {
        var saveResult = await command.RunAsync(new Command { Id = 1, Data = "test" });
        Assert.True(saveResult.WasSuccessful);

        var res = await query.GetAsync(new Query { Id = 1 });
        Assert.NotNull(res);
        Assert.Equal("test", res);
    }

    public async Task InitializeAsync()
    {
        await app.InitializeAsync();

        query = app.CreateQueriesExecutor();
        command = app.CreateCommandsExecutor();
    }

    public Task DisposeAsync() => app.DisposeAsync().AsTask();
}
