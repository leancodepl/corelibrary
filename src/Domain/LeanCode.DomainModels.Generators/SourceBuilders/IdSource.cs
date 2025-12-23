namespace LeanCode.DomainModels.Generators.SourceBuilders;

internal static class IdSource
{
    public static string Build(TypedIdData data)
    {
        return data.Format switch
        {
            TypedIdFormat.RawInt => RawIdSourceBuilder.Build(
                data,
                "int",
                "Int",
                null,
                "0",
                "CultureInfo.InvariantCulture",
                "string.Empty, CultureInfo.InvariantCulture"
            ),
            TypedIdFormat.RawLong => RawIdSourceBuilder.Build(
                data,
                "long",
                "Long",
                null,
                "0",
                "CultureInfo.InvariantCulture",
                "string.Empty, CultureInfo.InvariantCulture"
            ),
            TypedIdFormat.RawGuid => RawIdSourceBuilder.Build(
                data,
                "Guid",
                "Guid",
                "Guid.CreateVersion7()",
                "Guid.Empty",
                "",
                "string.Empty"
            ),
            TypedIdFormat.RawString => RawStringIdSourceBuilder.Build(data),
            TypedIdFormat.PrefixedGuid => PrefixedGuidIdSourceBuilder.Build(data),
            TypedIdFormat.PrefixedUlid => PrefixedUlidIdSourceBuilder.Build(data),
            TypedIdFormat.PrefixedString => PrefixedStringIdSourceBuilder.Build(data),
            _ => throw new ArgumentException("Unsupported ID format."),
        };
    }

    public static string GetDefaultPrefix(string typeName)
    {
        typeName = typeName.ToLowerInvariant();

        if (typeName.EndsWith("id", StringComparison.OrdinalIgnoreCase))
        {
            typeName = typeName.Substring(0, typeName.Length - 2);
        }

        return typeName;
    }
}
