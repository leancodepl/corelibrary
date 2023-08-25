using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.MassTransitRelay.Tests.Middleware;

public static class TestHelpers
{
    public static Endpoint MockCQRSEndpoint(CQRSObjectMetadata obj)
    {
        var endpointMetadata = new CQRSEndpointMetadata(obj, (_, __) => Task.FromResult(null as object));
        return new Endpoint(null, new EndpointMetadataCollection(endpointMetadata), obj.ObjectType.Name);
    }
}
