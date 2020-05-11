using System.Threading.Tasks;
using Xunit;

namespace LeanCode.IntegrationTestHelpers.Tests
{
    public class Tests : IAsyncLifetime
    {
        private readonly AuthenticatedTestApp app;

        public Tests()
        {
            app = new AuthenticatedTestApp();
        }

        [Fact]
        public async Task TestQuery()
        {
            var res = await app.Query.GetAsync(new App.SampleQuery());

            Assert.Equal($"{App.AuthConfig.UserId}-", res?.Data);
        }

        public Task InitializeAsync() => app.InitializeAsync();
        public Task DisposeAsync() => app.DisposeAsync();
    }
}
