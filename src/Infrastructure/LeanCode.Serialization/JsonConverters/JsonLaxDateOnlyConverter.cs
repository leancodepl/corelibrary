using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeanCode.Serialization;

public class JsonLaxDateOnlyConverter : JsonConverter<DateOnly>
{
    private const int MaxBufferSize = 256;
    private const int MaxDateOnlyBufferSize = 16;

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

        if (DateTime.TryParse(source, out DateTime date))
        {
            return DateOnly.FromDateTime(date);
        }

        throw new JsonException("Invalid date format");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        Span<char> buffer = stackalloc char[MaxDateOnlyBufferSize];
        var success = value.TryFormat(buffer, out var written, "o", CultureInfo.InvariantCulture);
        writer.WriteStringValue(JsonEncodedText.Encode(buffer[..written], JavaScriptEncoder.UnsafeRelaxedJsonEscaping));
    }
}
