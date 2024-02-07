using FluentAssertions;
using LeanCode.CQRS.AspNetCore.Local.Context;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Local.Context;

public class LocalCallLifetimeFeatureTests
{
    [Fact]
    public void CallAborted_and_RequestAborted_are_linked_to_the_passed_cancellation_token()
    {
        using var cts = new CancellationTokenSource();
        using var feature = new LocalCallLifetimeFeature(cts.Token);

        feature.CallAborted.IsCancellationRequested.Should().BeFalse();
        feature.RequestAborted.IsCancellationRequested.Should().BeFalse();

        cts.Cancel();

        feature.CallAborted.IsCancellationRequested.Should().BeTrue();
        feature.RequestAborted.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public void Aborting_cancels_both_tokens()
    {
        using var cts = new CancellationTokenSource();
        using var feature = new LocalCallLifetimeFeature(cts.Token);

        feature.CallAborted.IsCancellationRequested.Should().BeFalse();
        feature.RequestAborted.IsCancellationRequested.Should().BeFalse();

        feature.Abort();

        feature.CallAborted.IsCancellationRequested.Should().BeTrue();
        feature.RequestAborted.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public void RequestAborted_can_be_changed()
    {
        using var cts = new CancellationTokenSource();
        using var feature = new LocalCallLifetimeFeature(cts.Token);

        feature.RequestAborted = new();
        feature.Abort();

        feature.RequestAborted.IsCancellationRequested.Should().BeFalse();
        feature.CallAborted.IsCancellationRequested.Should().BeTrue();
    }
}
