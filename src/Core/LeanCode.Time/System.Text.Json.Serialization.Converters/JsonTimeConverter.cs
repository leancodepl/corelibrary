using System.Globalization;

namespace System.Text.Json.Serialization.Converters
{
    public class JsonTimeConverter : JsonConverter<Time>
    {
        public override Time Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString() is string s
                ? Time.Parse(s, CultureInfo.InvariantCulture)
                : default;
        }

        public override void Write(Utf8JsonWriter writer, Time value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
    }
}
