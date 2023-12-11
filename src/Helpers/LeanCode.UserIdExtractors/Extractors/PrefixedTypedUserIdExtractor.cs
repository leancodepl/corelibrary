using System.Security.Claims;
using LeanCode.DomainModels.Ids;

namespace LeanCode.UserIdExtractors.Extractors;

public sealed class PrefixedTypedUserIdExtractor<TId> : IUserIdExtractor<TId>
    where TId : struct, IPrefixedTypedId<TId>
{
    private readonly string userIdClaim;

    public PrefixedTypedUserIdExtractor(string userIdClaim)
    {
        this.userIdClaim = userIdClaim;
    }

    public TId Extract(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(userIdClaim)?.Value;
        ArgumentException.ThrowIfNullOrEmpty(claim);

        return TId.Parse(claim);
    }
}
