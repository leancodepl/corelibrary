using LeanCode.DomainModels.Ids;

namespace LeanCode.DomainModels.EF.Tests;

[TypedId(TypedIdFormat.RawInt)]
public readonly partial record struct IntId;

[TypedId(TypedIdFormat.RawLong)]
public readonly partial record struct LongId;

[TypedId(TypedIdFormat.RawGuid)]
public readonly partial record struct GuidId;

[TypedId(TypedIdFormat.RawString, MaxValueLength = 100)]
public readonly partial record struct StringId;

[TypedId(TypedIdFormat.PrefixedGuid)]
public readonly partial record struct PrefixedGuidId;

[TypedId(TypedIdFormat.PrefixedUlid)]
public readonly partial record struct PrefixedUlidId;

[TypedId(TypedIdFormat.PrefixedString, MaxValueLength = 100)]
public readonly partial record struct PrefixedStringId;
