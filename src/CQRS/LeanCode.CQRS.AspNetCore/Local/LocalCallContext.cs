using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Features.Authentication;

namespace LeanCode.CQRS.AspNetCore.Local;

internal class LocalCallContext : HttpContext
{
    private const int DefaultFeatureCollectionSize = 5;

    private readonly FeatureCollection features;

    private readonly ItemsFeature itemsFeature;
    private readonly LocalCallServiceProvidersFeature serviceProvidersFeature;
    private readonly HttpAuthenticationFeature httpAuthenticationFeature;
    private readonly LocalCallLifetimeFeature callLifetimeFeature;
    private readonly LocalCallIdentifier callIdentifier;

    public LocalCallContext(
        IServiceProvider requestServices,
        ClaimsPrincipal? claimsPrincipal,
        string activityIdentifier,
        CancellationToken cancellationToken
    )
    {
        features = new(DefaultFeatureCollectionSize);

        itemsFeature = new();
        serviceProvidersFeature = new(requestServices);
        httpAuthenticationFeature = new() { User = claimsPrincipal };
        callLifetimeFeature = new(cancellationToken);
        callIdentifier = new(activityIdentifier ?? "");

        features.Set<IItemsFeature>(itemsFeature);
        features.Set<IServiceProvidersFeature>(serviceProvidersFeature);
        features.Set<IHttpAuthenticationFeature>(httpAuthenticationFeature);
        features.Set<IHttpRequestLifetimeFeature>(callLifetimeFeature);
        features.Set<IHttpRequestIdentifierFeature>(callIdentifier);
    }

    public override IFeatureCollection Features => features;
    public override HttpRequest Request => throw new NotImplementedException();
    public override HttpResponse Response => throw new NotImplementedException();

    public override ClaimsPrincipal User
    {
        get => httpAuthenticationFeature.User!;
        set => httpAuthenticationFeature.User = value ?? new();
    }
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
    public override string TraceIdentifier
    {
        get => callIdentifier.TraceIdentifier;
        set => callIdentifier.TraceIdentifier = value;
    }

    public override ConnectionInfo Connection =>
        throw new NotSupportedException("There is no Connection in local CQRS calls.");
    public override WebSocketManager WebSockets =>
        throw new NotSupportedException("WebSockets are not supported in local CQRS calls.");
    public override ISession Session
    {
        get => throw new NotSupportedException("There is no Session in local CQRS calls.");
        set => throw new NotSupportedException("There is no Session in local CQRS calls.");
    }

    public override void Abort() => callLifetimeFeature.Abort();
}
