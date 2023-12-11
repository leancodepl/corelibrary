using System.Security.Claims;

namespace LeanCode.UserIdExtractors.Extractors;

public class StringUserIdExtractor : IUserIdExtractor
{
    private readonly string userIdClaim;

    public StringUserIdExtractor(string userIdClaim)
    {
        this.userIdClaim = userIdClaim;
    }

    public string Extract(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(userIdClaim)?.Value;
        ArgumentException.ThrowIfNullOrEmpty(claim);

        return claim;
    }
}

public sealed class GenericStringUserIdExtractor : StringUserIdExtractor, IUserIdExtractor<string>
{
    public GenericStringUserIdExtractor(string userIdClaim)
        : base(userIdClaim) { }
}
