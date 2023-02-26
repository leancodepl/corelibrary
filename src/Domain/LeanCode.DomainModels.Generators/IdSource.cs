namespace LeanCode.DomainModels.Generators;

internal static class IdSource
{
    public static string Build(TypedIdData data)
    {
        switch (data.Format)
        {
            case TypedIdFormat.RawInt:
                return BuildRaw(data, "int", "Int", null, "0", "CultureInfo.InvariantCulture");

            case TypedIdFormat.RawLong:
                return BuildRaw(data, "long", "Long", null, "0", "CultureInfo.InvariantCulture");

            case TypedIdFormat.RawGuid:
                return BuildRaw(data, "Guid", "Guid", "Guid.NewGuid()", "Guid.Empty", "");

            case TypedIdFormat.PrefixedGuid:
                return BuildPrefixed(data, "Guid.NewGuid()", 32, "Guid.Empty", true, "Guid.TryParseExact", "N");

            default:
                throw new ArgumentException("Unsupported ID format.");
        }
    }

    private static string BuildPrefixed(
        TypedIdData data,
        string randomValueGenerator,
        int valueLength,
        string defaultValue,
        bool generateFromCtor,
        string tryParseExact,
        string rawFormat
    )
    {
        var prefix = data.CustomPrefix?.ToLowerInvariant() ?? GetDefaultPrefix(data.TypeName);
        var fromCtor = generateFromCtor
            ? $@"public {data.TypeName}(Guid v) => value = string.Create(null, stackalloc char[RawLength], $""{{TypePrefix}}{{Separator}}{{v:{rawFormat}}}"");"
            : "";
        return $@"// <auto-generated />
#nullable enable
namespace {data.Namespace}
{{
    using global::System;
    using global::System.Diagnostics;
    using global::System.Diagnostics.CodeAnalysis;
    using global::System.Linq.Expressions;
    using global::System.Text.Json.Serialization;
    using global::LeanCode.DomainModels.Ids;

    [JsonConverter(typeof(StringTypedIdConverter<{data.TypeName}>))]
    [DebuggerDisplay(""{{Value}}"")]
    public readonly partial record struct {data.TypeName} : IPrefixedTypedId<{data.TypeName}>
    {{
        private const int ValueLength = {valueLength};
        private const char Separator = '_';
        private const string TypePrefix = ""{prefix}"";

        public static int RawLength {{ get; }} = ValueLength + 1 + {prefix.Length};
        public static readonly {data.TypeName} Empty = new({defaultValue});

        private readonly string? value;

        public string Value => value ?? Empty.Value;
        public bool IsEmpty => value is null || value == Empty;

        private {data.TypeName}(string v) => value = v;
        public static {data.TypeName} New() => new({data.CustomGenerator ?? randomValueGenerator});
        {fromCtor}

        public static {data.TypeName} Parse(string? v)
        {{
            if (IsValid(v))
            {{
                return new {data.TypeName}(v);
            }}
            else
            {{
                throw new FormatException(
                    $""The ID has invalid format. It should look like {{TypePrefix}}{{Separator}}(id value).""
                );
            }}
        }}

        [return: NotNullIfNotNull(""id"")]
        public static {data.TypeName}? ParseNullable(string? id) => id is string v ? Parse(v) : ({data.TypeName}?)null;

        public static bool TryParse([NotNullWhen(true)] string? v, out {data.TypeName} id)
        {{
            if (IsValid(v))
            {{
                id = new {data.TypeName}(v);
                return true;
            }}
            else
            {{
                id = default;
                return false;
            }}
        }}

        public static bool IsValid([NotNullWhen(true)] string? v)
        {{
            if (v is null)
            {{
                return false;
            }}
            else
            {{
                var span = v.AsSpan();
                return span.Length == RawLength
                    && span.StartsWith(TypePrefix)
                    && span[{prefix.Length}] == Separator
                    && {tryParseExact}(span[({prefix.Length + 1})..], ""{rawFormat}"", out _);
            }}
        }}

        public static Expression<Func<string, {data.TypeName}>> FromDatabase {{ get; }} = d => Parse(d);

        public bool Equals({data.TypeName} other) => Value.Equals(other.Value, StringComparison.Ordinal);
        public int CompareTo({data.TypeName} other) => string.Compare(Value, other.Value, StringComparison.Ordinal);
        public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
        public override string ToString() => Value;
        public static implicit operator string({data.TypeName} id) => id.Value;

        public static bool operator <({data.TypeName} a, {data.TypeName} b) => a.CompareTo(b) < 0;
        public static bool operator <=({data.TypeName} a, {data.TypeName} b) => a.CompareTo(b) <= 0;
        public static bool operator >({data.TypeName} a, {data.TypeName} b) => a.CompareTo(b) > 0;
        public static bool operator >=({data.TypeName} a, {data.TypeName} b) => a.CompareTo(b) >= 0;
    }}
}}";
    }

    private static string BuildRaw(
        TypedIdData data,
        string backingType,
        string converterPrefix,
        string? randomValueGenerator,
        string defaultValue,
        string toStringParam
    )
    {
        var randomFactory = randomValueGenerator is not null
            ? $@"public static {data.TypeName} New() => new({randomValueGenerator});"
            : "";
        return $@"// <auto-generated />
#nullable enable
namespace {data.Namespace}
{{
    using global::System;
    using global::System.Diagnostics;
    using global::System.Diagnostics.CodeAnalysis;
    using global::System.Globalization;
    using global::System.Linq.Expressions;
    using global::System.Text.Json.Serialization;
    using global::LeanCode.DomainModels.Ids;

    [JsonConverter(typeof({converterPrefix}TypedIdConverter<{data.TypeName}>))]
    [DebuggerDisplay(""{{Value}}"")]
    public readonly partial record struct {data.TypeName} : IRawTypedId<{backingType}, {data.TypeName}>
    {{
        public static readonly {data.TypeName} Empty = new({defaultValue});

        public {backingType} Value {{ get; }}
        public bool IsEmpty => Value == Empty;

        public {data.TypeName}({backingType} v) => Value = v;
        {randomFactory}

        public static {data.TypeName} Parse({backingType}? v)
        {{
            if (IsValid(v))
            {{
                return new {data.TypeName}(v.Value);
            }}
            else
            {{
                throw new FormatException(""The ID has invalid format. It should be a valid `{backingType}`."");
            }}
        }}

        [return: NotNullIfNotNull(""id"")]
        public static {data.TypeName}? ParseNullable({backingType}? id) => id is {backingType} v ? Parse(v) : ({data.TypeName}?)null;

        public static bool TryParse([NotNullWhen(true)] {backingType}? v, out {data.TypeName} id)
        {{
            if (IsValid(v))
            {{
                id = new {data.TypeName}(v.Value);
                return true;
            }}
            else
            {{
                id = default;
                return false;
            }}
        }}

        public static bool IsValid([NotNullWhen(true)] {backingType}? v)
        {{
            return v is not null;
        }}

        public static Expression<Func<{backingType}, {data.TypeName}>> FromDatabase {{ get; }} = d => Parse(d);

        public bool Equals({data.TypeName} other) => Value == other.Value;
        public int CompareTo({data.TypeName} other) => Value.CompareTo(other.Value);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString({toStringParam});
        public static implicit operator {backingType}({data.TypeName} id) => id.Value;

        public static bool operator <({data.TypeName} a, {data.TypeName} b) => a.Value < b.Value;
        public static bool operator <=({data.TypeName} a, {data.TypeName} b) => a.Value <= b.Value;
        public static bool operator >({data.TypeName} a, {data.TypeName} b) => a.Value > b.Value;
        public static bool operator >=({data.TypeName} a, {data.TypeName} b) => a.Value >= b.Value;
    }}
}}";
    }

    private static string GetDefaultPrefix(string typeName)
    {
        typeName = typeName.ToLowerInvariant();

        if (typeName.EndsWith("id", StringComparison.OrdinalIgnoreCase))
        {
            typeName = typeName.Substring(0, typeName.Length - 2);
        }

        return typeName;
    }
}
