using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeanCode.Serialization;

public class JsonTimeOnlyConverter : JsonConverter<TimeOnly>
{
    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString() is string s
            ? TimeOnly.ParseExact(s, "o", CultureInfo.InvariantCulture)
            : default;
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString("o", CultureInfo.InvariantCulture));
}
