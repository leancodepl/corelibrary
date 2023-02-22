using System.Threading.Tasks;
using Xunit;

namespace LeanCode.CQRS.Tests;

public class AuthorizerResolvingTests : BaseCQRSTests
{
    public AuthorizerResolvingTests()
    {
        Prepare();
    }

    [Fact]
    public void Resolves_existing_authorizer_for_command()
    {
        var (handler, underlying) = FindSampleAuthorizer<SampleCommand>();

        Assert.NotNull(handler);
        Assert.NotNull(underlying);
    }

    [Fact]
    public void Resolves_existing_authorizer_for_query()
    {
        var (handler, underlying) = FindSampleAuthorizer<SampleQuery>();

        Assert.NotNull(handler);
        Assert.NotNull(underlying);
    }

    [Fact]
    public void Resolves_different_authorizers_for_different_calls()
    {
        var (handler1, underlying1) = FindSampleAuthorizer<SampleCommand>();
        var (handler2, underlying2) = FindSampleAuthorizer<SampleCommand>();

        Assert.NotSame(handler1, handler2);
        Assert.NotSame(underlying1, underlying2);
    }

    [Fact]
    public async Task Passes_correct_data_to_underlying_authorizer()
    {
        var appCtx = new AppContext();
        var cmd = new SampleCommand();
        var data = new object();

        var (handler, underlying) = FindSampleAuthorizer<SampleCommand>();
        underlying.Result = true;

        var result = await handler.CheckIfAuthorizedAsync(appCtx, cmd, data);

        Assert.Same(appCtx, underlying.AppContext);
        Assert.Same(cmd, underlying.Object);
        Assert.Same(data, underlying.Data);
        Assert.True(result);
    }
}
