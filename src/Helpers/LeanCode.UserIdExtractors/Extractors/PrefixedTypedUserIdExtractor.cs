using System.Security.Claims;
using LeanCode.DomainModels.Ids;

namespace LeanCode.UserIdExtractors.Extractors;

internal class PrefixedTypedUserIdExtractor<TId> : StringUserIdExtractor, IUserIdExtractor<TId>
    where TId : struct, IPrefixedTypedId<TId>
{
    public PrefixedTypedUserIdExtractor(string userIdClaim)
        : base(userIdClaim) { }

    public TId ExtractId(ClaimsPrincipal user)
    {
        return TId.Parse(Extract(user));
    }
}
