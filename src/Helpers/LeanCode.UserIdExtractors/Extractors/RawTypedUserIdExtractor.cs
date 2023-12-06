using System.Globalization;
using System.Security.Claims;
using LeanCode.DomainModels.Ids;

namespace LeanCode.UserIdExtractors.Extractors;

internal sealed class RawTypedUserIdExtractor<TBacking, TId> : StringUserIdExtractor, IUserIdExtractor<TId>
    where TBacking : struct
    where TId : struct, IRawTypedId<TBacking, TId>
{
    private static readonly Dictionary<Type, Func<string, object>> Parsers = new Dictionary<Type, Func<string, object>>
    {
        { typeof(int), s => int.Parse(s, CultureInfo.InvariantCulture) },
        { typeof(long), s => long.Parse(s, CultureInfo.InvariantCulture) },
        { typeof(Guid), s => Guid.Parse(s) }
    };

    public RawTypedUserIdExtractor(string userIdClaim)
        : base(userIdClaim) { }

    public TId ExtractId(ClaimsPrincipal user)
    {
        var id = Extract(user);
        var backing = GetBacking(id);

        return TId.Parse(backing);
    }

    private static TBacking GetBacking(string value)
    {
        var backingType = typeof(TBacking);
        var parsedValue = Parsers[backingType].Invoke(value);

        return (TBacking)Convert.ChangeType(parsedValue, typeof(TBacking), CultureInfo.InvariantCulture);
    }
}
