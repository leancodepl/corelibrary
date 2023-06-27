using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeanCode.Serialization;

public sealed class JsonCamelCaseStringEnumConverter : JsonStringEnumConverter
{
    public JsonCamelCaseStringEnumConverter()
        : base(JsonNamingPolicy.CamelCase, false) { }
}

public sealed class JsonKebabCaseLowerStringEnumConverter : JsonStringEnumConverter
{
    public JsonKebabCaseLowerStringEnumConverter()
        : base(JsonNamingPolicy.KebabCaseLower, false) { }
}

public sealed class JsonKebabCaseUpperStringEnumConverter : JsonStringEnumConverter
{
    public JsonKebabCaseUpperStringEnumConverter()
        : base(JsonNamingPolicy.KebabCaseUpper, false) { }
}

public sealed class JsonSnakeCaseLowerStringEnumConverter : JsonStringEnumConverter
{
    public JsonSnakeCaseLowerStringEnumConverter()
        : base(JsonNamingPolicy.SnakeCaseLower, false) { }
}

public sealed class JsonSnakeCaseUpperStringEnumConverter : JsonStringEnumConverter
{
    public JsonSnakeCaseUpperStringEnumConverter()
        : base(JsonNamingPolicy.SnakeCaseUpper, false) { }
}
