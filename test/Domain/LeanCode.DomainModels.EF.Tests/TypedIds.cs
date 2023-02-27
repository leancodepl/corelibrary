using LeanCode.DomainModels.Ids;

namespace LeanCode.DomainModels.EF.Tests;

[TypedId(TypedIdFormat.RawInt)]
public readonly partial record struct IntId;

[TypedId(TypedIdFormat.RawLong)]
public readonly partial record struct LongId;

[TypedId(TypedIdFormat.RawGuid)]
public readonly partial record struct GuidId;

[TypedId(TypedIdFormat.PrefixedGuid)]
public readonly partial record struct PrefixedGuidId;
