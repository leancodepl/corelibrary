using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeanCode.Serialization;

public class JsonLaxTimeOnlyConverter : JsonConverter<TimeOnly>
{
    private const int MaxBufferSize = 256;
    private const int MaxTimeOnlyBufferSize = 16;
    private readonly string format = "HH:mm:ss.fff";

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

        if (TimeOnly.TryParse(source, out TimeOnly time))
        {
            return new TimeOnly(time.Hour, time.Minute, time.Second, time.Millisecond);
        }

        throw new JsonException("Invalid format");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        Span<char> buffer = stackalloc char[MaxTimeOnlyBufferSize];
        var success = value.TryFormat(buffer, out var written, format, CultureInfo.InvariantCulture);
        writer.WriteStringValue(JsonEncodedText.Encode(buffer[..written], JavaScriptEncoder.UnsafeRelaxedJsonEscaping));
    }
}
