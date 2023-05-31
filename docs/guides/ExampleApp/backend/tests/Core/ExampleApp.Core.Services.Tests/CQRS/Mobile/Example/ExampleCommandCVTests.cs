using ExampleApp.Core.Services.CQRS.Example;
using Xunit;

namespace ExampleApp.Core.Services.Tests.CQRS.Example;

public class ExampleCommandCVTests
{
    private readonly ExampleCommandCV validator;

    public ExampleCommandCVTests()
    {
        validator = new ExampleCommandCV();
    }

    [Fact]
    public void ImplementMe()
    {
        Assert.True(true);
    }
}
