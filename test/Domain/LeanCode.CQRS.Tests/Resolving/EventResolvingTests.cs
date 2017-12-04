using System.Threading.Tasks;
using Xunit;

namespace LeanCode.CQRS.Tests
{
    public class EventResolvingTests : BaseCQRSTests
    {
        public EventResolvingTests()
        {
            Prepare();
        }

        [Fact]
        public void Resolves_handler()
        {
            var (handler, underlying) = FindSampleEventHandler();

            Assert.NotNull(handler);
            Assert.NotNull(underlying);
        }

        [Fact]
        public void Resolves_different_handlers_for_subsequent_calls()
        {
            var (handler1, underlying1) = FindSampleEventHandler();
            var (handler2, underlying2) = FindSampleEventHandler();

            Assert.NotSame(handler1, handler2);
            Assert.NotSame(underlying1, underlying2);
        }

        [Fact]
        public void Resolves_empty_collection_when_there_are_not_handlers()
        {
            var handlers = EventResolver.FindEventHandlers(typeof(SampleEvent2));

            Assert.Empty(handlers);
        }

        [Fact]
        public async Task Passes_event_data_to_underlying_handler()
        {
            var evt = new SampleEvent();

            var (handler, underlying) = FindSampleEventHandler();

            await handler.HandleAsync(evt);

            Assert.Equal(evt, underlying.Event);
        }
    }
}
