using System.Security.Claims;

namespace LeanCode.UserIdExtractors.Extractors;

internal class StringUserIdExtractor : IUserIdExtractor
{
    private readonly string userIdClaim;

    public StringUserIdExtractor(string userIdClaim)
    {
        this.userIdClaim = userIdClaim;
    }

    public string Extract(ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue(userIdClaim);

        ArgumentException.ThrowIfNullOrEmpty(claim);

        return claim;
    }
}

internal sealed class GenericStringUserIdExtractor : StringUserIdExtractor, IUserIdExtractor<string>
{
    public GenericStringUserIdExtractor(string userIdClaim)
        : base(userIdClaim) { }

    public string ExtractId(ClaimsPrincipal user) => Extract(user);
}
