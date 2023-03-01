using LeanCode.DomainModels.Ids;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeanCode.DomainModels.EF;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class PrefixedTypedIdConverter<TId> : ValueConverter<TId, string>
    where TId : struct, IPrefixedTypedId<TId>
{
    public static readonly PrefixedTypedIdConverter<TId> Instance = new();

    public PrefixedTypedIdConverter()
        : base(d => d.Value, TId.FromDatabase, mappingHints: null) { }
}

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class RawTypedIdConverter<TBacking, TId> : ValueConverter<TId, TBacking>
    where TBacking : struct
    where TId : struct, IRawTypedId<TBacking, TId>
{
    public static readonly RawTypedIdConverter<TBacking, TId> Instance = new();

    public RawTypedIdConverter()
        : base(d => d.Value, TId.FromDatabase, mappingHints: null) { }
}

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class PrefixedTypedIdComparer<TId> : ValueComparer<TId>
    where TId : struct, IPrefixedTypedId<TId>
{
    public PrefixedTypedIdComparer()
        : base(TId.DatabaseEquals, d => d.GetHashCode()) { }
}

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class RawTypedIdComparer<TBacking, TId> : ValueComparer<TId>
    where TBacking : struct
    where TId : struct, IRawTypedId<TBacking, TId>
{
    public RawTypedIdComparer()
        : base(TId.DatabaseEquals, d => d.GetHashCode()) { }
}
