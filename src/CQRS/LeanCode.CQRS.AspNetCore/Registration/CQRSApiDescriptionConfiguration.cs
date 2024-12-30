using System.Collections.Frozen;
using Microsoft.AspNetCore.Routing;

namespace LeanCode.CQRS.AspNetCore.Registration;

public class CQRSApiDescriptionConfiguration
{
    // Required by Swagger (Swashbuckle)
    public static readonly FrozenDictionary<string, string?> DefaultRouteValues = new Dictionary<string, string?>
    {
        ["controller"] = "CQRS",
    }.ToFrozenDictionary();

    public Func<RouteEndpoint, IDictionary<string, string?>> RouteValuesMapping { get; init; } =
        _ => DefaultRouteValues;
}
