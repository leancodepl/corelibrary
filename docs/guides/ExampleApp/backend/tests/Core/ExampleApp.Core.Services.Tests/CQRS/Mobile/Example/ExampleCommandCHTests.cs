using ExampleApp.Core.Services.CQRS.Example;
using Xunit;

namespace ExampleApp.Core.Services.Tests.CQRS.Example;

public class ExampleCommandCHTests
{
    private readonly ExampleCommandCH handler;

    public ExampleCommandCHTests()
    {
        handler = new ExampleCommandCH();
    }

    [Fact]
    public void ImplementMe()
    {
        Assert.True(true);
    }
}
