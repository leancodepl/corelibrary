using System.Security.Claims;

namespace LeanCode.UserIdExtractors;

public interface IUserIdExtractor<TUserId> : IUserIdExtractor
    where TUserId : notnull, IEquatable<TUserId>
{
    new TUserId Extract(ClaimsPrincipal user);
    string IUserIdExtractor.Extract(ClaimsPrincipal user) => Extract(user).ToString()!;
}

public interface IUserIdExtractor
{
    string Extract(ClaimsPrincipal user);
}
