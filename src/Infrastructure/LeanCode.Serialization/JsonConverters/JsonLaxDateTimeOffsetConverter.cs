using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeanCode.Serialization;

public class JsonLaxDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    private static readonly UTF8Encoding encoder = new(false);
    private const int maxBufferSize = 32;
    private readonly string format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffZ";

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Span<char> source = stackalloc char[0];

        if(reader.HasValueSequence)
        {
            var valueLength = (int)reader.ValueSequence.Length;
            Span<char> buffer = ArrayPool<char>.Shared.Rent(valueLength);
            int charsRead = reader.CopyString(buffer);
            source = buffer.Slice(0, charsRead);
        }
        else
        {
            var value = reader.ValueSpan;
            source  = stackalloc char[value.Length];
            encoder.GetChars(value, source);
        }

        if (DateTimeOffset.TryParse(source, CultureInfo.InvariantCulture, out DateTimeOffset date))
        {
            return new DateTimeOffset(date.Ticks, date.Offset);
        }

        throw new JsonException("Invalid format");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) {
        Span<char> buffer = stackalloc char[32];
        var success = value.TryFormat(buffer, out var charsWritten, format, CultureInfo.InvariantCulture);
        writer.WriteStringValue(buffer[..charsWritten]);
    }

}
