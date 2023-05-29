using Autofac;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.Integration;

/// <remarks>
/// Single integration test checking if events from command handler and further
/// event handlers (consumers) are raised
/// </remarks>
[Collection("EventsInterceptor")]
public class MassTransitIntegrationTest : IClassFixture<TestApp>
{
    private readonly Guid correlationId = Guid.NewGuid();
    private readonly TestApp testApp;

    public MassTransitIntegrationTest(TestApp testApp)
    {
        this.testApp = testApp;
    }

    [Fact]
    public async Task Test_event_relay_and_handling()
    {
        await ExecuteCommandAsync();
        await VerifyEventRaisedFromCommandWasPublishedAndHandled();
        await VerifyEventsFromConsumersWereHandled();
        await VerifyHandledLogs();
    }

    private async Task VerifyHandledLogs()
    {
        await using var scope = testApp.Container.BeginLifetimeScope();
        await using var dbContext = scope.Resolve<TestDbContext>();

        var handled = await dbContext.HandledLog.Where(l => l.CorrelationId == correlationId).ToListAsync();

        Assert.Contains(handled, h => h.HandlerName == nameof(TestCommandHandler));
        Assert.Contains(handled, h => h.HandlerName == nameof(Event1Consumer));
        Assert.Contains(handled, h => h.HandlerName == nameof(Event2FirstConsumer));
        Assert.Contains(handled, h => h.HandlerName == nameof(Event2SecondConsumer));
        Assert.Contains(handled, h => h.HandlerName == nameof(Event2RetryingConsumer));
    }

    private async Task ExecuteCommandAsync()
    {
        var ctx = new Context();
        var cmd = new TestCommand { CorrelationId = correlationId };
        // await testApp.Commands.RunAsync(ctx, cmd);
        // TODO: handled in other PR
    }

    private async Task VerifyEventRaisedFromCommandWasPublishedAndHandled()
    {
        await WaitForConsumers();

        var consumer = testApp.Harness.GetConsumerHarness<Event1Consumer>();
        var consumed = await consumer.Consumed.Any();
        Assert.True(consumed);
    }

    private async Task VerifyEventsFromConsumersWereHandled()
    {
        await WaitForConsumers();

        var firstConsumer = testApp.Harness.GetConsumerHarness<Event2FirstConsumer>();
        var secondConsumer = testApp.Harness.GetConsumerHarness<Event2SecondConsumer>();
        var thirdConsumer = testApp.Harness.GetConsumerHarness<Event2RetryingConsumer>();

        var firstConsumed = await firstConsumer.Consumed.Any();
        var secondConsumed = await secondConsumer.Consumed.Any();
        var thirdConsumed = await thirdConsumer.Consumed.Any();

        Assert.True(firstConsumed);
        Assert.True(secondConsumed);
        Assert.True(thirdConsumed);
    }

    private async Task WaitForConsumers()
    {
        // 5 because of the retries + inactivity timeout + some buffer
        var result = await testApp.ActivityMonitor.AwaitBusInactivity(TimeSpan.FromSeconds(10));
        Assert.True(result, "The bus did not stabilize.");
    }
}
