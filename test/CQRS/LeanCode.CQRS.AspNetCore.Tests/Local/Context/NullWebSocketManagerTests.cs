using FluentAssertions;
using LeanCode.CQRS.AspNetCore.Local.Context;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Local.Context;

public class NullWebSocketManagerTests
{
    [Fact]
    public void IsWebSocketRequest_should_always_be_false()
    {
        NullWebSocketManager.Empty.IsWebSocketRequest.Should().BeFalse();
    }

    [Fact]
    public void Request_protocols_should_be_empty_and_readonly()
    {
        NullWebSocketManager.Empty.WebSocketRequestedProtocols.Should().BeEmpty();
        NullWebSocketManager.Empty.WebSocketRequestedProtocols.IsReadOnly.Should().BeTrue();

        var act = () => NullWebSocketManager.Empty.WebSocketRequestedProtocols.Add("");
        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public async Task Accept_should_always_throw()
    {
        var act = () => NullWebSocketManager.Empty.AcceptWebSocketAsync((string?)null);
        await act.Should().ThrowAsync<NotSupportedException>();
    }
}
