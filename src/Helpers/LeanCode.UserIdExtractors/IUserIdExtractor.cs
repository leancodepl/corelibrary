using System.Security.Claims;

namespace LeanCode.UserIdExtractors;

public interface IUserIdExtractor<TUserId> : IUserIdExtractor
    where TUserId : notnull, IEquatable<TUserId>
{
    TUserId ExtractId(ClaimsPrincipal user);
}

public interface IUserIdExtractor
{
    string Extract(ClaimsPrincipal user);
}
