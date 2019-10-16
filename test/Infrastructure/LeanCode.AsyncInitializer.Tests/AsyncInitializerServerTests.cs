using System.Threading.Tasks;
using Xunit;

namespace LeanCode.AsyncInitializer.Tests
{
    public class AsyncInitializerServerTests
    {
        private readonly Counter counter;

        private readonly CountedInitializer initializer;
        private readonly CountedServer innerServer;
        private readonly AsyncInitializerServer server;

        public AsyncInitializerServerTests()
        {
            counter = new Counter();

            initializer = new CountedInitializer(0, counter);
            innerServer = new CountedServer(counter);
            server = new AsyncInitializerServer(new AsyncInitializer(new[] { initializer }), innerServer);
        }

        [Fact]
        public async Task StartAsync_calls_the_StartAsync_of_inner_server()
        {
            await server.StartAsync<object>(null, default);

            Assert.NotNull(innerServer.StartOrder);
        }

        [Fact]
        public async Task StopAsync_calls_the_StopAsync_of_inner_server()
        {
            await server.StopAsync(default);

            Assert.NotNull(innerServer.StopOrder);
        }

        [Fact]
        public async Task StartAsync_calls_the_initializer()
        {
            await server.StartAsync<object>(default, default);

            Assert.NotNull(initializer.InitOrder);
        }

        [Fact]
        public async Task StopAsync_calls_the_initializer()
        {
            await server.StopAsync(default);

            Assert.NotNull(initializer.DeinitOrder);
        }

        [Fact]
        public async Task StartAsync_calls_the_initializer_before_inner_server()
        {
            await server.StartAsync<object>(null, default);

            Assert.Equal(0, initializer.InitOrder);
            Assert.Equal(1, innerServer.StartOrder);
        }

        [Fact]
        public async Task StopAsync_calls_the_initializer_after_inner_server()
        {
            await server.StopAsync(default);

            Assert.Equal(0, innerServer.StopOrder);
            Assert.Equal(1, initializer.DeinitOrder);
        }

        [Fact]
        public void Dispose_passes_through_to_the_inner_server()
        {
            server.Dispose();

            Assert.NotNull(innerServer.DisposeOrder);
        }

        [Fact]
        public void Features_is_passed_through_from_inner_server()
        {
            Assert.Equal(innerServer.Features, server.Features);
        }
    }
}
