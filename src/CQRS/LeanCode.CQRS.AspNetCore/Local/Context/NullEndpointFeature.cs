using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace LeanCode.CQRS.AspNetCore.Local.Context;

internal class NullEndpointFeature : IEndpointFeature
{
    public static readonly NullEndpointFeature Empty = new();

    public Endpoint? Endpoint
    {
        get => null;
        set { }
    }

    private NullEndpointFeature() { }
}
