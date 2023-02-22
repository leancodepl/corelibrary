using System.Threading.Tasks;
using Xunit;

namespace LeanCode.CQRS.Tests;

public class CommandResolvingTests : BaseCQRSTests
{
    public CommandResolvingTests()
    {
        Prepare();
    }

    [Fact]
    public void Resolves_CH_for_existing_command()
    {
        var (handler, underlying) = FindSampleCommandHandler();

        Assert.NotNull(handler);
        Assert.NotNull(underlying);
    }

    [Fact]
    public void Resolves_diffrent_CH_if_called_multiple_times_for_existing_command()
    {
        var (handler1, _) = FindSampleCommandHandler();
        var (handler2, _) = FindSampleCommandHandler();

        Assert.NotNull(handler1);
        Assert.NotNull(handler2);
        Assert.NotSame(handler1, handler2);
    }

    [Fact]
    public void Resolves_different_instances_of_underlying_CH_when_resolving_the_same_command_multiple_times()
    {
        var (_, underlying1) = FindSampleCommandHandler();
        var (_, underlying2) = FindSampleCommandHandler();

        Assert.NotNull(underlying1);
        Assert.NotNull(underlying2);
        Assert.NotSame(underlying1, underlying2);
    }

    [Fact]
    public void The_wrapper_type_is_the_same_when_resolved_the_same_command_multiple_times()
    {
        var (handler1, _) = FindSampleCommandHandler();
        var (handler2, _) = FindSampleCommandHandler();

        Assert.Same(handler1.GetType(), handler2.GetType());
    }

    [Fact]
    public void Resolves_null_if_CH_is_not_found()
    {
        var handler = CHResolver.FindCommandHandler(typeof(NoCHCommand));

        Assert.Null(handler);
    }

    [Fact]
    public async Task The_data_is_correctly_passed_to_underlying_CH()
    {
        var ctx = new AppContext();
        var cmd = new SampleCommand();

        var (handler, underlying) = FindSampleCommandHandler();

        await handler.ExecuteAsync(ctx, cmd);

        Assert.Equal(cmd, underlying.Command);
        Assert.Equal(ctx, underlying.Context);
    }
}
