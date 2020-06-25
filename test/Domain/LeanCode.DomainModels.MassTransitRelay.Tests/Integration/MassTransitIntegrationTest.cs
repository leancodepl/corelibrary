using System;
using System.Linq;
using System.Threading.Tasks;
using LeanCode.DomainModels.MassTransitRelay.Inbox;
using LeanCode.DomainModels.MassTransitRelay.Outbox;
using LeanCode.IntegrationTestHelpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.Integration
{
    /// <remarks>
    /// Single intergration test checking if events from command handler and further
    /// event handlers (consumers) are raised
    /// </remarks>
    public class MassTransitIntegrationTest : IClassFixture<TestApp>
    {
        private readonly TestApp testApp;

        public MassTransitIntegrationTest(TestApp testApp)
        {
            this.testApp = testApp;
        }

        [Fact]
        public async Task Test_event_relay_and_handling()
        {
            await PublishEvents();
            await TestEventsFromCommandHandler();
            await TestEventsFromConsumers();
            await TestFailingHandlers();
            await AssertRaisedEventsWerePeristedAndMarkedPublished();
            await AssertConsumedEventsWerePersisted();
        }

        private async Task PublishEvents()
        {
            var ctx = new Context { CorrelationId = testApp.CorrelationId };
            var cmd = new TestCommand();
            await testApp.Commands.RunAsync(ctx, cmd);
        }

        private async Task TestEventsFromCommandHandler()
        {
            await WaitForConsumers();
            var handled = testApp.HandledEvents<Event1>();

            var evt = Assert.Single(handled);
            AssertConsumed(evt, typeof(Event1Consumer));
        }

        private async Task TestEventsFromConsumers()
        {
            await WaitForConsumers();
            var handled = testApp.HandledEvents<Event2>();

            Assert.Collection(
                handled.OrderBy(x => x.ConsumerType.FullName),
                e => AssertConsumed(e, typeof(Event2FirstConsumer)),
                e => AssertConsumed(e, typeof(Event2SecondConsumer)));
        }

        private async Task TestFailingHandlers()
        {
            await Task.Delay(5_500);

            var handled = testApp.HandledEvents<Event3>();
            var evt = Assert.Single(handled);

            AssertConsumed(evt, typeof(Event3RetryingConsumer));
        }

        private async Task AssertRaisedEventsWerePeristedAndMarkedPublished()
        {
            using var dbContext = new TestDbContext(testApp.DbConnection);
            var raisedEvents = await dbContext.RaisedEvents
                .OrderBy(e => e.EventType)
                .ToListAsync();

            Assert.Collection(
                raisedEvents,
                evt => AssertRaisedEvent(evt, typeof(Event1)),
                evt => AssertRaisedEvent(evt, typeof(Event2)),
                evt => AssertRaisedEvent(evt, typeof(Event3)));
        }

        private async Task AssertConsumedEventsWerePersisted()
        {
            using var dbContext = new TestDbContext(testApp.DbConnection);

            var consumedMsgs = await dbContext.ConsumedMessages
                .OrderBy(e => e.ConsumerType)
                .ThenBy(e => e.MessageType)
                .ToListAsync();

            Assert.Collection(
                consumedMsgs,
                msg => AssertConsumedMessage(msg, typeof(Event1Consumer), typeof(Event1)),
                msg => AssertConsumedMessage(msg, typeof(Event2FirstConsumer), typeof(Event2)),
                msg => AssertConsumedMessage(msg, typeof(Event2SecondConsumer), typeof(Event2)),
                msg => AssertConsumedMessage(msg, typeof(Event3RetryingConsumer), typeof(Event3)));
        }

        private void AssertConsumed(HandledEvent evt, Type consumerType)
        {
            Assert.Equal(consumerType, evt.ConsumerType);
            Assert.Equal(testApp.CorrelationId, evt.CorrelationId);
        }

        private void AssertConsumedMessage(ConsumedMessage msg, Type consumerType, Type messageType)
        {
            Assert.Equal(consumerType.FullName, msg.ConsumerType);
            Assert.Equal(messageType.FullName, msg.MessageType);
        }

        private void AssertRaisedEvent(RaisedEvent evt, Type type)
        {
            Assert.Equal(type.FullName, evt.EventType);
            Assert.True(evt.WasPublished);
        }

        private Task WaitForConsumers() => Task.Delay(500);
    }
}
