using System;
using System.Linq;
using System.Threading.Tasks;
using LeanCode.IntegrationTestHelpers;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.Integration
{
    public class MassTransitIntegrationTest : IClassFixture<TestApp>
    {
        private readonly TestApp testApp;

        public MassTransitIntegrationTest(TestApp testApp)
        {
            this.testApp = testApp;
        }

        [PreparationStep(0)]
        public async Task Publishing_events_from_command()
        {
            var ctx = new Context { CorrelationId = testApp.CorrelationId };
            var cmd = new TestCommand();
            await testApp.Commands.RunAsync(ctx, cmd);
        }

        [TestStep(1)]
        public async Task Events_raised_directly_from_command_are_consumed()
        {
            await WaitForConsumers();
            var handled = testApp.HandledEvents<Event1>();

            var evt = Assert.Single(handled);
            Assert.Equal(typeof(FirstEvent1Consumer), evt.ConsumerType);
            Assert.Equal(testApp.CorrelationId, evt.CorrelationId);
        }

        [TestStep(2)]
        public async Task Events_raised_from_consumers_are_consumed_by_all_consumers()
        {
            await WaitForConsumers();
            var handled = testApp.HandledEvents<Event2>();

            Assert.Collection(
                handled.OrderBy(x => x.ConsumerType.FullName),
                e => AssertConsumed(e, typeof(Event2FirstConsumer)),
                e => AssertConsumed(e, typeof(Event2SecondConsumer)));
        }

        [TestStep(3)]
        public async Task Failing_handlers_are_retried()
        {
            await Task.Delay(5_500);

            var handled = testApp.HandledEvents<Event3>();
            var evt = Assert.Single(handled);

            AssertConsumed(evt, typeof(Event3RetryingConsumer));
        }

        private void AssertConsumed(HandledEvent evt, Type consumerType)
        {
            Assert.Equal(consumerType, evt.ConsumerType);
            Assert.Equal(testApp.CorrelationId, evt.CorrelationId);
        }

        private Task WaitForConsumers() => Task.Delay(500);
    }
}
