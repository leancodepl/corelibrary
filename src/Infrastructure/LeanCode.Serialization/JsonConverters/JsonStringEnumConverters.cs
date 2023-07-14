using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeanCode.Serialization;

public sealed class JsonCamelCaseStringEnumConverter<TEnum> : JsonStringEnumConverter<TEnum>
    where TEnum : struct, Enum
{
    public JsonCamelCaseStringEnumConverter()
        : base(JsonNamingPolicy.CamelCase, false) { }
}

public sealed class JsonKebabCaseLowerStringEnumConverter<TEnum> : JsonStringEnumConverter<TEnum>
    where TEnum : struct, Enum
{
    public JsonKebabCaseLowerStringEnumConverter()
        : base(JsonNamingPolicy.KebabCaseLower, false) { }
}

public sealed class JsonKebabCaseUpperStringEnumConverter<TEnum> : JsonStringEnumConverter<TEnum>
    where TEnum : struct, Enum
{
    public JsonKebabCaseUpperStringEnumConverter()
        : base(JsonNamingPolicy.KebabCaseUpper, false) { }
}

public sealed class JsonSnakeCaseLowerStringEnumConverter<TEnum> : JsonStringEnumConverter<TEnum>
    where TEnum : struct, Enum
{
    public JsonSnakeCaseLowerStringEnumConverter()
        : base(JsonNamingPolicy.SnakeCaseLower, false) { }
}

public sealed class JsonSnakeCaseUpperStringEnumConverter<TEnum> : JsonStringEnumConverter<TEnum>
    where TEnum : struct, Enum
{
    public JsonSnakeCaseUpperStringEnumConverter()
        : base(JsonNamingPolicy.SnakeCaseUpper, false) { }
}
