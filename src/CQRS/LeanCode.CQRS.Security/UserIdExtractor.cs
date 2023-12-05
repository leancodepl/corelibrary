using System.Security.Claims;

namespace LeanCode.CQRS.Security;

public interface IUserIdExtractor<TUserId> : IUserIdExtractor
    where TUserId : notnull, IEquatable<TUserId>
{
    new TUserId Extract(ClaimsPrincipal user);
}

public interface IUserIdExtractor
{
    string Extract(ClaimsPrincipal user);
}

public class UserIdExtractor : IUserIdExtractor<Guid>
{
    protected virtual string UserIdClaim => "sub";

    public Guid Extract(ClaimsPrincipal user) => Guid.Parse(((IUserIdExtractor)this).Extract(user));

    string IUserIdExtractor.Extract(ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue(UserIdClaim);

        ArgumentException.ThrowIfNullOrEmpty(claim);

        return claim;
    }
}
