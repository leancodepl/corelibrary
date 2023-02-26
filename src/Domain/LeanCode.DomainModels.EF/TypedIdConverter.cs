using LeanCode.DomainModels.Ids;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeanCode.DomainModels.EF;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class PrefixedTypedIdConverter<TId> : ValueConverter<TId, string>
    where TId : struct, IPrefixedTypedId<TId>
{
    public static readonly PrefixedTypedIdConverter<TId> Instance = new();

    private PrefixedTypedIdConverter()
        : base(d => d.Value, TId.FromDatabase, mappingHints: null) { }
}

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class RawTypedIdConverter<TBacking, TId> : ValueConverter<TId, TBacking>
    where TBacking : struct
    where TId : struct, IRawTypedId<TBacking, TId>
{
    public static readonly RawTypedIdConverter<TBacking, TId> Instance = new();

    private RawTypedIdConverter()
        : base(d => d.Value, TId.FromDatabase, mappingHints: null) { }
}
