using System.Globalization;

namespace System.Text.Json.Serialization.Converters
{
    public class JsonDateConverter : JsonConverter<Date>
    {
        public override Date Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            Date.Parse(reader.GetString(), CultureInfo.InvariantCulture);

        public override void Write(Utf8JsonWriter writer, Date value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToIsoString());
    }
}
