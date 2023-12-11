using System.Collections.Frozen;
using System.Globalization;
using System.Security.Claims;
using LeanCode.DomainModels.Ids;

namespace LeanCode.UserIdExtractors.Extractors;

public sealed class RawTypedUserIdExtractor<TBacking, TId> : IUserIdExtractor<TId>
    where TBacking : struct
    where TId : struct, IRawTypedId<TBacking, TId>
{
    private readonly string userIdClaim;

    private static readonly FrozenDictionary<Type, Func<string, object>> Parsers = new Dictionary<
        Type,
        Func<string, object>
    >()
    {
        { typeof(int), s => int.Parse(s, CultureInfo.InvariantCulture) },
        { typeof(long), s => long.Parse(s, CultureInfo.InvariantCulture) },
        { typeof(Guid), s => Guid.Parse(s) }
    }.ToFrozenDictionary();

    public RawTypedUserIdExtractor(string userIdClaim)
    {
        this.userIdClaim = userIdClaim;
    }

    public TId Extract(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(userIdClaim)?.Value;
        ArgumentException.ThrowIfNullOrEmpty(claim);

        var backing = GetBacking(claim);
        return TId.Parse(backing);
    }

    private static TBacking GetBacking(string value)
    {
        var backingType = typeof(TBacking);
        var parsedValue = Parsers[backingType].Invoke(value);

        return (TBacking)parsedValue;
    }
}
