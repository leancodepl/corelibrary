using System.Security.Claims;

namespace LeanCode.UserIdExtractors.Extractors;

public sealed class GuidUserIdExtractor : IUserIdExtractor<Guid>
{
    private readonly string userIdClaim;

    public GuidUserIdExtractor(string userIdClaim)
    {
        this.userIdClaim = userIdClaim;
    }

    public Guid Extract(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(userIdClaim)?.Value;
        ArgumentException.ThrowIfNullOrEmpty(claim);

        return Guid.Parse(claim);
    }
}
