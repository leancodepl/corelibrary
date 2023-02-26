using LeanCode.DomainModels.Ids;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeanCode.DomainModels.EF;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class PrefixedTypedIdConverter<TBacking, TId> : ValueConverter<TId, TBacking>
    where TBacking : class
    where TId : struct, IPrefixedTypedId<TBacking, TId>
{
    public static readonly PrefixedTypedIdConverter<TBacking, TId> Instance = new();

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
