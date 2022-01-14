using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeanCode.Serialization;

public class JsonDateOnlyConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString() is string s
            ? DateOnly.ParseExact(s, "o", CultureInfo.InvariantCulture)
            : default;
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString("o", CultureInfo.InvariantCulture));
}