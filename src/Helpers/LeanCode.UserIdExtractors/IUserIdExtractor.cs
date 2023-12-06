using System.Security.Claims;

namespace LeanCode.UserIdExtractors;

public interface IUserIdExtractor<TUserId> : IUserIdExtractor
    where TUserId : notnull, IEquatable<TUserId>
{
    new TUserId Extract(ClaimsPrincipal user);
}

public interface IUserIdExtractor
{
    string Extract(ClaimsPrincipal user);
}
