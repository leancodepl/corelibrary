using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeanCode.Serialization;

public class JsonLaxDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    private const int MaxBufferSize = 256;
    private const int MaxDateTimeOffsetBufferSize = 32;
    private readonly string format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffzzz";

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        int length;

        if (reader.HasValueSequence)
        {
            length = (int)reader.ValueSequence.Length;
        }
        else
        {
            length = reader.ValueSpan.Length;
        }

        if (length > MaxBufferSize)
        {
            throw new JsonException($"Input too long ({length})");
        }

        Span<char> buffer = stackalloc char[length];
        int charsRead = reader.CopyString(buffer);
        Span<char> source = buffer.Slice(0, charsRead);

        if (DateTimeOffset.TryParse(source, CultureInfo.InvariantCulture, out DateTimeOffset date))
        {
            return date.AddTicks(-(date.Ticks % TimeSpan.TicksPerMillisecond));
        }

        throw new JsonException("Invalid format");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        Span<char> buffer = stackalloc char[MaxDateTimeOffsetBufferSize];
        var success = value.TryFormat(buffer, out var written, format, CultureInfo.InvariantCulture);
        writer.WriteStringValue(JsonEncodedText.Encode(buffer[..written], JavaScriptEncoder.UnsafeRelaxedJsonEscaping));
    }
}
