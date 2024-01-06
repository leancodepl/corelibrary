using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace LeanCode.CQRS.AspNetCore.Local;

internal class LocalHttpContext : HttpContext
{
    private readonly HttpContext inner;
    private readonly LocalFeatureCollection features;

    private IServiceProvider requestServices;
    private CancellationToken requestAborted;

    public LocalHttpContext(HttpContext inner, IServiceProvider requestServices, CancellationToken requestAborted)
    {
        this.inner = inner;
        this.requestServices = requestServices;
        this.requestAborted = requestAborted;

        features = new LocalFeatureCollection(inner.Features);
    }

    public override HttpResponse Response =>
        throw new NotSupportedException("Accessing response is not supported for Local calls.");
    public override IFeatureCollection Features => features;
    public override IServiceProvider RequestServices
    {
        get => requestServices;
        set => requestServices = value;
    }

    public override HttpRequest Request => inner.Request;
    public override ConnectionInfo Connection => inner.Connection;
    public override WebSocketManager WebSockets => inner.WebSockets;
    public override ClaimsPrincipal User
    {
        get => inner.User;
        set => inner.User = value;
    }
    public override IDictionary<object, object?> Items
    {
        get => inner.Items;
        set => inner.Items = value;
    }
    public override CancellationToken RequestAborted
    {
        get => requestAborted;
        set => requestAborted = value;
    }
    public override string TraceIdentifier
    {
        get => inner.TraceIdentifier;
        set => inner.TraceIdentifier = value;
    }
    public override ISession Session
    {
        get => inner.Session;
        set => inner.Session = value;
    }

    public override void Abort() => throw new InvalidOperationException("Not supported for now.");
}
