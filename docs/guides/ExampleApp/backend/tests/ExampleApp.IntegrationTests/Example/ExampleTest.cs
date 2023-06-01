using ExampleApp.Core.Contracts.Projects;
using Xunit;

namespace ExampleApp.IntegrationTests.Example
{
    public class ExampleTest : TestsBase<UnauthenticatedExampleAppTestApp>
    {
        [Fact]
        public async Task Example_test()
        {
            var result = await App.Command.RunAsync(new CreateProject { Name = "Project", });

            Assert.True(result.WasSuccessful);
        }
    }
}
