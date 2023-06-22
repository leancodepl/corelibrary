using FluentAssertions;
using FluentAssertions.Execution;
using LeanCode.CQRS.RemoteHttp.Client;
using LeanCode.IntegrationTestHelpers;
using LeanCode.IntegrationTests.App;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.IntegrationTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1001", Justification = "Disposed with `IAsyncLifetime`.")]
public class Tests : IAsyncLifetime
{
    private readonly TestApp app;

    public Tests()
    {
        app = new TestApp();
    }

    [Fact]
    public async Task Test_basic_cqrs_flows()
    {
        var bus = app.Services.GetRequiredService<IBusControl>();
        var probe = bus.GetProbeResult();

        await RunUnauthorizedCommandsAsync();
        await RunInvalidCommandAsync();
        await AddEntityAndVerifyResultsAsync();
    }

    private async Task RunUnauthorizedCommandsAsync()
    {
        var unauthorizedCall = () => app.CreateCommandsExecutor().RunAsync(new AddEntity());
        await unauthorizedCall.Should().ThrowAsync<UnauthorizedException>();

        var forbiddenCall = () =>
            app.CreateCommandsExecutor(client => client.UseTestAuthorization(AuthConfig.UserWithoutRole))
                .RunAsync(new AddEntity());
        await forbiddenCall.Should().ThrowAsync<ForbiddenException>();
    }

    private async Task RunInvalidCommandAsync()
    {
        var result = await app.CreateCommandsExecutor(client => client.UseTestAuthorization(AuthConfig.User))
            .RunAsync(new AddEntity());

        using var _ = new AssertionScope();
        result.WasSuccessful.Should().BeFalse();
        result.ValidationErrors
            .Should()
            .ContainSingle()
            .Which.ErrorCode.Should()
            .Be(AddEntity.ErrorCodes.ValueRequired);
    }

    private async Task AddEntityAndVerifyResultsAsync()
    {
        var cmdResult = await app.CreateCommandsExecutor(client => client.UseTestAuthorization(AuthConfig.User))
            .RunAsync(new AddEntity { Value = "test-entity" });
        cmdResult.WasSuccessful.Should().BeTrue();

        await app.WaitForBusAsync();

        var entities = await app.CreateQueriesExecutor(client => client.UseTestAuthorization(AuthConfig.User))
            .GetAsync(new ListEntities());

        entities
            .Should()
            .Satisfy(
                e1 => e1.Value == "test-entity" && e1.Id != Guid.Empty,
                e2 => e2.Value == "test-entity-consumer" && e2.Id != Guid.Empty
            );
    }

    public Task InitializeAsync() => app.InitializeAsync();

    public Task DisposeAsync() => app.DisposeAsync().AsTask();
}
