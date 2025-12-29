using Microsoft.CodeAnalysis;

namespace LeanCode.DomainModels.Generators;

public readonly struct TypedIdData : IEquatable<TypedIdData>
{
    public TypedIdFormat Format { get; }
    public string Namespace { get; }
    public string TypeName { get; }
    public string? CustomPrefix { get; }
    public bool SkipRandomGenerator { get; }
    public int? MaxValueLength { get; }
    public bool IsValid { get; }

    // Excluded from equality comparison because it's not stable across compilations.
    public Location? Location { get; }

    public TypedIdData(
        TypedIdFormat format,
        string @namespace,
        string typeName,
        string? customPrefix,
        bool skipRandomGenerator,
        int? maxValueLength,
        bool isValid,
        Location? location
    )
    {
        Format = format;
        Namespace = @namespace;
        TypeName = typeName;
        CustomPrefix = customPrefix;
        SkipRandomGenerator = skipRandomGenerator;
        MaxValueLength = maxValueLength;
        IsValid = isValid;
        Location = location;
    }

    public bool Equals(TypedIdData other) =>
        (Format, Namespace, TypeName, CustomPrefix, SkipRandomGenerator, MaxValueLength, IsValid)
        == (
            other.Format,
            other.Namespace,
            other.TypeName,
            other.CustomPrefix,
            other.SkipRandomGenerator,
            other.MaxValueLength,
            other.IsValid
        );

    public override bool Equals(object? obj) => obj is TypedIdData other && Equals(other);

    public override int GetHashCode() =>
        (Format, Namespace, TypeName, CustomPrefix, SkipRandomGenerator, MaxValueLength, IsValid).GetHashCode();

    public static bool operator ==(TypedIdData left, TypedIdData right) => left.Equals(right);

    public static bool operator !=(TypedIdData left, TypedIdData right) => !left.Equals(right);
}
