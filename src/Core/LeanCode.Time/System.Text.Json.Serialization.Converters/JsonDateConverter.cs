using System.Globalization;

namespace System.Text.Json.Serialization.Converters
{
    public class JsonDateConverter : JsonConverter<Date>
    {
        public override Date Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString() is string s
                ? Date.Parse(s, CultureInfo.InvariantCulture)
                : default;
        }

        public override void Write(Utf8JsonWriter writer, Date value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToIsoString());
    }
}
