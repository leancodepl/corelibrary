using ExampleApp.Core.Contracts.Example;
using Xunit;

namespace ExampleApp.IntegrationTests.Example
{
    public class ExampleTest : TestsBase<UnauthenticatedExampleAppTestApp>
    {
        [Fact]
        public async Task Example_test()
        {
            var result = await App.Command.RunAsync(new ExampleCommand
            {
                Arg = "foo",
            });

            Assert.True(result.WasSuccessful);
        }
    }
}
