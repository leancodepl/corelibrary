using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Routing;

namespace LeanCode.CQRS.AspNetCore.Registration;

public static class ApiDescriptionTags
{
    public static IReadOnlyList<string> FullNamespace(RouteEndpoint _, CQRSObjectMetadata m)
    {
        return [(m.ObjectType.Namespace ?? "").Replace(".", " - ", StringComparison.InvariantCulture)];
    }

    public static Func<RouteEndpoint, CQRSObjectMetadata, IReadOnlyList<string>> SkipNamespacePrefix(int skipFirst)
    {
        return (_, m) =>
        {
            var parts = (m.ObjectType.Namespace ?? "").Split('.').Skip(skipFirst);
            return [string.Join(" - ", parts)];
        };
    }
}
