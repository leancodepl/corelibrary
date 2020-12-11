using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LeanCode.OrderedHostedServices.Tests
{
    public class OrderedHostedServiceExecutorTests
    {
        private readonly Counter globalCounter = new();

        [Fact]
        public async Task StartAsync_calls_StartAsync_on_every_object()
        {
            var (inits, executor) = Prepare(
                Init(0),
                Init(1),
                Init(2));

            await executor.StartAsync(default);

            Assert.NotNull(inits[0].StartOrder);
            Assert.NotNull(inits[1].StartOrder);
            Assert.NotNull(inits[2].StartOrder);
        }

        [Fact]
        public async Task StopAsync_calls_StopAsync_on_every_object()
        {
            var (inits, executor) = Prepare(
                Init(0),
                Init(1),
                Init(2));

            await executor.StopAsync(default);

            Assert.NotNull(inits[0].StopOrder);
            Assert.NotNull(inits[1].StopOrder);
            Assert.NotNull(inits[2].StopOrder);
        }

        [Fact]
        public async Task StopAsync_orders_the_objects_in_reverse_order_before_calling_StopAsync()
        {
            var (inits, executor) = Prepare(
                Init(1),
                Init(2),
                Init(0));

            await executor.StopAsync(default);

            Assert.Equal(1, inits[0].StopOrder);
            Assert.Equal(0, inits[1].StopOrder);
            Assert.Equal(2, inits[2].StopOrder);
        }

        [Fact]
        public async Task StartAsync_orders_the_objects_before_calling_StartAsync()
        {
            var (inits, executor) = Prepare(
                Init(1),
                Init(2),
                Init(0));

            await executor.StartAsync(default);

            Assert.Equal(1, inits[0].StartOrder);
            Assert.Equal(2, inits[1].StartOrder);
            Assert.Equal(0, inits[2].StartOrder);
        }

        private static (CountedInitializer[], OrderedHostedServiceExecutor) Prepare(params CountedInitializer[] inits)
        {
            return (inits, new OrderedHostedServiceExecutor(inits));
        }

        private CountedInitializer Init(int order)
        {
            return new CountedInitializer(order, globalCounter);
        }
    }
}
