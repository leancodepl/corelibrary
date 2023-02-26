using Microsoft.CodeAnalysis;

namespace LeanCode.DomainModels.Generators;

public sealed class TypedIdData
{
    public TypedIdFormat Format { get; }
    public string Namespace { get; }
    public string TypeName { get; }
    public string? CustomPrefix { get; }
    public string? CustomGenerator { get; }
    public bool IsValid { get; }
    public Location? Location { get; }

    public TypedIdData(
        TypedIdFormat format,
        string @namespace,
        string typeName,
        string? customSlug,
        string? customGenerator,
        bool isValid,
        Location? location
    )
    {
        Format = format;
        Namespace = @namespace;
        TypeName = typeName;
        CustomPrefix = customSlug;
        CustomGenerator = customGenerator;
        IsValid = isValid;
        Location = location;
    }
}
