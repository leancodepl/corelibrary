using System.Threading.Tasks;
using LeanCode.CQRS.RemoteHttp.Client;
using Xunit;

namespace LeanCode.IntegrationTestHelpers.Tests
{
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
        public async Task Save_and_load()
        {
            var saveResult = await command.RunAsync(new App.Command { Id = 1, Data = "test" });
            Assert.True(saveResult.WasSuccessful);

            var res = await query.GetAsync(new App.Query { Id = 1 });
            Assert.NotNull(res);
            Assert.Equal("test", res);
        }

        public async Task InitializeAsync()
        {
            await app.InitializeAsync();

            query = app.CreateQueriesExecutor();
            command = app.CreateCommandsExecutor();
        }

        public Task DisposeAsync() => app.DisposeAsync();
    }
}
