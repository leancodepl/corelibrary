using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace LeanCode.CQRS.AspNetCore.Local.Context;

internal class LocalCallContext : HttpContext
{
    private const int DefaultFeatureCollectionSize = 6; // 4 internal, 2 external (set in local executors)

    private readonly FeatureCollection features;

    private readonly ItemsFeature itemsFeature;
    private readonly LocalCallServiceProvidersFeature serviceProvidersFeature;
    private readonly LocalCallLifetimeFeature callLifetimeFeature;

    public override IFeatureCollection Features => features;

    public override ClaimsPrincipal User { get; set; }
    public override string TraceIdentifier { get; set; }
    public override HttpRequest Request { get; }
    public override HttpResponse Response { get; }

    public override IDictionary<object, object?> Items
    {
        get => itemsFeature.Items;
        set => itemsFeature.Items = value;
    }

    public override IServiceProvider RequestServices
    {
        get => serviceProvidersFeature.RequestServices;
        set => serviceProvidersFeature.RequestServices = value;
    }

    public override CancellationToken RequestAborted
    {
        get => callLifetimeFeature.RequestAborted;
        set => callLifetimeFeature.RequestAborted = value;
    }

    public CancellationToken CallAborted => callLifetimeFeature.CallAborted;

    public override ConnectionInfo Connection => NullConnectionInfo.Empty;
    public override WebSocketManager WebSockets => NullWebSocketManager.Empty;
    public override ISession Session
    {
        get => NullSession.Empty;
        set { }
    }

    public LocalCallContext(
        IServiceProvider requestServices,
        ClaimsPrincipal user,
        string? activityIdentifier,
        IHeaderDictionary? headers,
        CancellationToken cancellationToken
    )
    {
        features = new(DefaultFeatureCollectionSize);

        User = user;
        TraceIdentifier = activityIdentifier ?? "";
        Request = new LocalHttpRequest(this, headers);
        Response = new NullHttpResponse(this);

        itemsFeature = new();
        serviceProvidersFeature = new(requestServices);
        callLifetimeFeature = new(cancellationToken);

        features.Set<IItemsFeature>(itemsFeature);
        features.Set<IServiceProvidersFeature>(serviceProvidersFeature);
        features.Set<IHttpRequestLifetimeFeature>(callLifetimeFeature);
        features.Set<IEndpointFeature>(NullEndpointFeature.Empty);
    }

    public override void Abort() => callLifetimeFeature.Abort();
}
