using System.Security.Claims;

namespace LeanCode.UserIdExtractors.Extractors;

internal sealed class GuidUserIdExtractor : StringUserIdExtractor, IUserIdExtractor<Guid>
{
    public GuidUserIdExtractor(string userIdClaim)
        : base(userIdClaim) { }

    public Guid ExtractId(ClaimsPrincipal user)
    {
        return Guid.Parse(Extract(user));
    }
}
