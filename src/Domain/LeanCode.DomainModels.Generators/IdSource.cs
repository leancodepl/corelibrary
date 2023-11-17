namespace LeanCode.DomainModels.Generators;

internal static class IdSource
{
    public static string Build(TypedIdData data)
    {
        switch (data.Format)
        {
            case TypedIdFormat.RawInt:
                return BuildRaw(
                    data,
                    "int",
                    "Int",
                    null,
                    "0",
                    "CultureInfo.InvariantCulture",
                    "string.Empty, CultureInfo.InvariantCulture"
                );

            case TypedIdFormat.RawLong:
                return BuildRaw(
                    data,
                    "long",
                    "Long",
                    null,
                    "0",
                    "CultureInfo.InvariantCulture",
                    "string.Empty, CultureInfo.InvariantCulture"
                );

            case TypedIdFormat.RawGuid:
                return BuildRaw(data, "Guid", "Guid", "Guid.NewGuid()", "Guid.Empty", "", "string.Empty");

            case TypedIdFormat.PrefixedGuid:
                return BuildPrefixedGuid(data);

            case TypedIdFormat.PrefixedUlid:
                return BuildPrefixedUlid(data);

            default:
                throw new ArgumentException("Unsupported ID format.");
        }
    }

    private static string BuildPrefixedGuid(TypedIdData data)
    {
        var prefix = data.CustomPrefix?.ToLowerInvariant() ?? GetDefaultPrefix(data.TypeName);
        var valueLength = 32;

        var randomFactory = !data.SkipRandomGenerator
            ? $"public static {data.TypeName} New() => new(Guid.NewGuid());"
            : "";
        return $$"""
// <auto-generated />
#nullable enable
namespace {{data.Namespace}}
{
    #pragma warning disable CS8019
    using global::System;
    using global::System.ComponentModel;
    using global::System.Diagnostics.CodeAnalysis;
    using global::System.Diagnostics;
    using global::System.Linq.Expressions;
    using global::System.Text;
    using global::System.Text.Json.Serialization;
    using global::LeanCode.DomainModels.Ids;
    #pragma warning restore CS8019

    [JsonConverter(typeof(StringTypedIdConverter<{{data.TypeName}}>))]
    [DebuggerDisplay("{Value}")]
    [ExcludeFromCodeCoverage]
    public readonly partial record struct {{data.TypeName}} : IPrefixedTypedId<{{data.TypeName}}>
    {
        private const int ValueLength = {{valueLength}};
        private const char Separator = '_';
        private const string TypePrefix = "{{prefix}}";

        public static int RawLength { get; } = {{valueLength + 1 + prefix.Length}};
        public static readonly {{data.TypeName}} Empty = new(Guid.Empty);

        private readonly string? value;

        public string Value => value ?? Empty.Value;
        public bool IsEmpty => value is null || value == Empty;

        private {{data.TypeName}}(string v) => value = v;
        public {{data.TypeName}}(Guid v) => value = string.Create(null, stackalloc char[RawLength], $"{TypePrefix}{Separator}{v:N}");
        {{randomFactory}}

        public static {{data.TypeName}} Parse(string v)
        {
            if (IsValid(v))
            {
                return new {{data.TypeName}}(v);
            }
            else
            {
                throw new FormatException(
                    $"The ID has invalid format. It should look like {TypePrefix}{Separator}(id value)."
                );
            }
        }

        [return: NotNullIfNotNull("id")]
        public static {{data.TypeName}}? ParseNullable(string? id) => id is string v ? Parse(v) : ({{data.TypeName}}?)null;

        public static bool TryParse([NotNullWhen(true)] string? v, out {{data.TypeName}} id)
        {
            if (IsValid(v))
            {
                id = new {{data.TypeName}}(v);
                return true;
            }
            else
            {
                id = default;
                return false;
            }
        }

        public static bool IsValid([NotNullWhen(true)] string? v)
        {
            if (v is null)
            {
                return false;
            }
            else
            {
                var span = v.AsSpan();
                return span.Length == RawLength
                    && span.StartsWith(TypePrefix)
                    && span[{{prefix.Length}}] == Separator
                    && Guid.TryParseExact(span[{{prefix.Length + 1}}..], "N", out _);
            }
        }

        public bool Equals({{data.TypeName}} other) => Value.Equals(other.Value, StringComparison.Ordinal);
        public int CompareTo({{data.TypeName}} other) => string.Compare(Value, other.Value, StringComparison.Ordinal);
        public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
        public static implicit operator string({{data.TypeName}} id) => id.Value;

        public static bool operator <({{data.TypeName}} a, {{data.TypeName}} b) => a.CompareTo(b) < 0;
        public static bool operator <=({{data.TypeName}} a, {{data.TypeName}} b) => a.CompareTo(b) <= 0;
        public static bool operator >({{data.TypeName}} a, {{data.TypeName}} b) => a.CompareTo(b) > 0;
        public static bool operator >=({{data.TypeName}} a, {{data.TypeName}} b) => a.CompareTo(b) >= 0;

        static Expression<Func<string, {{data.TypeName}}>> IPrefixedTypedId<{{data.TypeName}}>.FromDatabase { get; } = d => Parse(d);
        static Expression<Func<{{data.TypeName}}, {{data.TypeName}}, bool>> IPrefixedTypedId<{{data.TypeName}}>.DatabaseEquals { get; } = (a, b) => a == b;

        public override string ToString() => Value;
        public string ToString(string? format, IFormatProvider? formatProvider) => Value;
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            if (destination.Length >= Value.Length)
            {
                Value.CopyTo(destination);
                charsWritten = Value.Length;
                return true;
            }
            else
            {
                charsWritten = 0;
                return false;
            }
        }

        public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
            => Encoding.UTF8.TryGetBytes(Value, utf8Destination, out bytesWritten);
    }
}
""";
    }

    private static string BuildPrefixedUlid(TypedIdData data)
    {
        var prefix = data.CustomPrefix?.ToLowerInvariant() ?? GetDefaultPrefix(data.TypeName);
        var valueLength = 26;

        return $$"""
// <auto-generated />
#nullable enable
namespace {{data.Namespace}}
{
    #pragma warning disable CS8019
    using global::System;
    using global::System.ComponentModel;
    using global::System.Diagnostics.CodeAnalysis;
    using global::System.Diagnostics;
    using global::System.Linq.Expressions;
    using global::System.Text;
    using global::System.Text.Json.Serialization;
    using global::LeanCode.DomainModels.Ids;
    using global::LeanCode.DomainModels.Ulids;
    #pragma warning restore CS8019

    [JsonConverter(typeof(StringTypedIdConverter<{{data.TypeName}}>))]
    [DebuggerDisplay("{Value}")]
    [ExcludeFromCodeCoverage]
    public readonly partial record struct {{data.TypeName}} : IPrefixedTypedId<{{data.TypeName}}>
    {
        private const int ValueLength = {{valueLength}};
        private const char Separator = '_';
        private const string TypePrefix = "{{prefix}}";

        public static int RawLength { get; } = {{valueLength + 1 + prefix.Length}};
        public static readonly {{data.TypeName}} Empty = new(Ulid.Empty);

        private readonly string? value;

        public string Value => value ?? Empty.Value;
        public bool IsEmpty => value is null || value == Empty;
        public Ulid Ulid => value is null ? Ulid.Empty : Ulid.Parse(value.AsSpan()[{{prefix.Length + 1}}..]);

        private {{data.TypeName}}(string v) => value = v;

        public {{data.TypeName}}(Ulid v) => value = string.Create(null, stackalloc char[RawLength], $"{TypePrefix}{Separator}{v}");

        public static {{data.TypeName}} New() => new(Ulid.NewUlid());

        public static {{data.TypeName}} Parse(string v)
        {
            if (TryDeconstruct(v.AsSpan(), out var ulid))
            {
                return new {{data.TypeName}}(ulid);
            }
            else
            {
                throw new FormatException(
                    $"The ID has invalid format. It should look like {TypePrefix}{Separator}(id value)."
                );
            }
        }

        [return: NotNullIfNotNull("id")]
        public static {{data.TypeName}}? ParseNullable(string? id) => id is string v ? Parse(v) : ({{data.TypeName}}?)null;

        public static bool TryParse([NotNullWhen(true)] string? v, out {{data.TypeName}} id)
        {
            if (TryDeconstruct(v, out var ulid))
            {
                id = new {{data.TypeName}}(ulid);
                return true;
            }
            else
            {
                id = default;
                return false;
            }
        }

        public static bool TryDeconstruct(ReadOnlySpan<char> span, out Ulid rawUlid)
        {
            rawUlid = Ulid.Empty;

            return span.Length == RawLength
                && span.StartsWith(TypePrefix)
                && span[{{prefix.Length}}] == Separator
                && Ulid.TryParse(span[{{prefix.Length + 1}}..], out rawUlid);
        }

        public static bool IsValid([NotNullWhen(true)] string? v)
        {
            return TryDeconstruct(v.AsSpan(), out _);
        }

        public bool Equals({{data.TypeName}} other) => Value.Equals(other.Value, StringComparison.Ordinal);
        public int CompareTo({{data.TypeName}} other) => string.Compare(Value, other.Value, StringComparison.Ordinal);
        public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
        public static implicit operator string({{data.TypeName}} id) => id.Value;

        public static bool operator <({{data.TypeName}} a, {{data.TypeName}} b) => a.CompareTo(b) < 0;
        public static bool operator <=({{data.TypeName}} a, {{data.TypeName}} b) => a.CompareTo(b) <= 0;
        public static bool operator >({{data.TypeName}} a, {{data.TypeName}} b) => a.CompareTo(b) > 0;
        public static bool operator >=({{data.TypeName}} a, {{data.TypeName}} b) => a.CompareTo(b) >= 0;

        static Expression<Func<string, {{data.TypeName}}>> IPrefixedTypedId<{{data.TypeName}}>.FromDatabase { get; } = d => Parse(d);
        static Expression<Func<{{data.TypeName}}, {{data.TypeName}}, bool>> IPrefixedTypedId<{{data.TypeName}}>.DatabaseEquals { get; } = (a, b) => a == b;

        public override string ToString() => Value;
        public string ToString(string? format, IFormatProvider? formatProvider) => Value;
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            if (destination.Length >= Value.Length)
            {
                Value.CopyTo(destination);
                charsWritten = Value.Length;
                return true;
            }
            else
            {
                charsWritten = 0;
                return false;
            }
        }

        public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
            => Encoding.UTF8.TryGetBytes(Value, utf8Destination, out bytesWritten);
    }
}
""";
    }

    private static string BuildRaw(
        TypedIdData data,
        string backingType,
        string converterPrefix,
        string? randomValueGenerator,
        string defaultValue,
        string toStringParam,
        string tryFormatParams
    )
    {
        var randomFactory =
            !data.SkipRandomGenerator && randomValueGenerator is not null
                ? $"public static {data.TypeName} New() => new({randomValueGenerator});"
                : "";
        return $$"""
// <auto-generated />
#nullable enable
namespace {{data.Namespace}}
{
    #pragma warning disable CS8019
    using global::System;
    using global::System.ComponentModel;
    using global::System.Diagnostics.CodeAnalysis;
    using global::System.Diagnostics;
    using global::System.Globalization;
    using global::System.Linq.Expressions;
    using global::System.Text.Json.Serialization;
    using global::LeanCode.DomainModels.Ids;
    #pragma warning restore CS8019

    [JsonConverter(typeof({{converterPrefix}}TypedIdConverter<{{data.TypeName}}>))]
    [DebuggerDisplay("{Value}")]
    [ExcludeFromCodeCoverage]
    public readonly partial record struct {{data.TypeName}} : IRawTypedId<{{backingType}}, {{data.TypeName}}>
    {
        public static readonly {{data.TypeName}} Empty = new({{defaultValue}});

        public {{backingType}} Value {get;}
        public bool IsEmpty => Value == Empty;

        public {{data.TypeName}}({{backingType}} v) => Value = v;
        {{randomFactory}}

        public static {{data.TypeName}} Parse({{backingType}} v)
        {
            return new {{data.TypeName}}(v);
        }

        [return: NotNullIfNotNull("id")]
        public static {{data.TypeName}}? ParseNullable({{backingType}}? id) => id is {{backingType}} v ? Parse(v) : ({{data.TypeName}}?)null;

        public static bool TryParse([NotNullWhen(true)] {{backingType}}? v, out {{data.TypeName}} id)
        {
            if (IsValid(v))
            {
                id = new {{data.TypeName}}(v.Value);
                return true;
            }
            else
            {
                id = default;
                return false;
            }
        }

        public static bool IsValid([NotNullWhen(true)] {{backingType}}? v)
        {
            return v is not null;
        }

        public bool Equals({{data.TypeName}} other) => Value == other.Value;
        public int CompareTo({{data.TypeName}} other) => Value.CompareTo(other.Value);
        public override int GetHashCode() => Value.GetHashCode();
        public static implicit operator {{backingType}}({{data.TypeName}} id) => id.Value;

        public static bool operator <({{data.TypeName}} a, {{data.TypeName}} b) => a.Value < b.Value;
        public static bool operator <=({{data.TypeName}} a, {{data.TypeName}} b) => a.Value <= b.Value;
        public static bool operator >({{data.TypeName}} a, {{data.TypeName}} b) => a.Value > b.Value;
        public static bool operator >=({{data.TypeName}} a, {{data.TypeName}} b) => a.Value >= b.Value;

        static Expression<Func<{{backingType}}, {{data.TypeName}}>> IRawTypedId<{{backingType}}, {{data.TypeName}}>.FromDatabase { get; } = d => Parse(d);
        static Expression<Func<{{data.TypeName}}, {{data.TypeName}}, bool>> IRawTypedId<{{backingType}}, {{data.TypeName}}>.DatabaseEquals { get; } = (a, b) => a == b;

        public override string ToString() => Value.ToString({{toStringParam}});
        public string ToString(string? format, IFormatProvider? formatProvider) => ToString();
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
            => Value.TryFormat(destination, out charsWritten, {{tryFormatParams}});

        public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
            => Value.TryFormat(utf8Destination, out bytesWritten, {{tryFormatParams}});
    }
}
""";
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
