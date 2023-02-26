namespace LeanCode.DomainModels.Generators;

public sealed class TypedIdData
{
    public TypedIdFormat Format { get; }
    public string Namespace { get; }
    public string TypeName { get; }
    public string? CustomPrefix { get; }
    public string? CustomGenerator { get; }

    public TypedIdData(
        TypedIdFormat format,
        string @namespace,
        string typeName,
        string? customSlug,
        string? customGenerator
    )
    {
        Format = format;
        Namespace = @namespace;
        TypeName = typeName;
        CustomPrefix = customSlug;
        CustomGenerator = customGenerator;
    }
}
