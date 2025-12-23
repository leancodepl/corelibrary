using Microsoft.CodeAnalysis;

namespace LeanCode.DomainModels.Generators;

public sealed class TypedIdData
{
    public TypedIdFormat Format { get; }
    public string Namespace { get; }
    public string TypeName { get; }
    public string? CustomPrefix { get; }
    public bool SkipRandomGenerator { get; }
    public int? MaxValueLength { get; }
    public bool IsValid { get; }
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
}
