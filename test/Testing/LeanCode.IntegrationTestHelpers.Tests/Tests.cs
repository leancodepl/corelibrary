using System.Security.Claims;
using FluentAssertions;
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
        var hostedServices = app.Services.GetRequiredService<IEnumerable<IHostedService>>().ToList();

        hostedServices.ElementAtOrDefault(0).Should().BeOfType<ConnectionKeeper>();
        hostedServices.ElementAtOrDefault(1).Should().BeOfType<DbContextInitializer<TestDbContext>>();
    }

    [Fact]
    public async Task Save_and_load()
    {
        var saveResult = await command.RunAsync(new Command { Id = 1, Data = "test" });
        saveResult.WasSuccessful.Should().BeTrue();

        var res = await query.GetAsync(new Query { Id = 1 });
        res.Should().Be("test");
    }

    [Fact]
    public async Task Test_authorization_scheme_works()
    {
        var testPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity(
                new Claim[]
                {
                    new("sub", "test_id"),
                    new("role", "user"),
                    new("role", "admin"),
                    new("other_claim", "other_claim_value")
                },
                TestAuthenticationHandler.SchemeName,
                "sub",
                "role"
            )
        );

        var queries = app.CreateQueriesExecutor(client => client.UseTestAuthorization(testPrincipal));
        var authResult = await queries.GetAsync(new AuthQuery());

        authResult.IsAuthenticated.Should().BeTrue();
        authResult
            .Claims.Should()
            .BeEquivalentTo(
                new[]
                {
                    KeyValuePair.Create("sub", "test_id"),
                    KeyValuePair.Create("role", "user"),
                    KeyValuePair.Create("role", "admin"),
                    KeyValuePair.Create("other_claim", "other_claim_value")
                }
            );
    }

    public async Task InitializeAsync()
    {
        await app.InitializeAsync();

        query = app.CreateQueriesExecutor();
        command = app.CreateCommandsExecutor();
    }

    public Task DisposeAsync() => app.DisposeAsync().AsTask();
}
