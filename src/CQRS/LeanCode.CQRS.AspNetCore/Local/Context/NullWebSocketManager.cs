using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Local.Context;

internal class NullWebSocketManager : WebSocketManager
{
    public static readonly NullWebSocketManager Empty = new();

    public override bool IsWebSocketRequest => false;

    public override IList<string> WebSocketRequestedProtocols => [ ];

    public override Task<WebSocket> AcceptWebSocketAsync(string? subProtocol) =>
        throw new NotSupportedException("WebSockets are not supported in local CQRS calls.");
}
