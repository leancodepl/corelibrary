using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeanCode.DomainModels.Ids;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class StringTypedIdConverter<TId> : JsonConverter<TId>
    where TId : struct, IPrefixedTypedId<TId>
{
    public override TId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        TId.Parse(reader.GetString());

    public override void Write(Utf8JsonWriter writer, TId value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);

    public override TId ReadAsPropertyName(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => Read(ref reader, typeToConvert, options);

    public override void WriteAsPropertyName(Utf8JsonWriter writer, TId value, JsonSerializerOptions options) =>
        writer.WritePropertyName(value.Value);
}

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class IntTypedIdConverter<TId> : JsonConverter<TId>
    where TId : struct, IRawTypedId<int, TId>
{
    public override TId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        TId.Parse(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, TId value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.Value);

    public override TId ReadAsPropertyName(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) =>
        reader.GetString() is string s && int.TryParse(s, out var id)
            ? TId.Parse(id)
            : throw new JsonException($"Could not deserialize {typeToConvert.Name}");

    public override void WriteAsPropertyName(Utf8JsonWriter writer, TId value, JsonSerializerOptions options) =>
        writer.WritePropertyName(value.ToString()!);
}

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class LongTypedIdConverter<TId> : JsonConverter<TId>
    where TId : struct, IRawTypedId<long, TId>
{
    public override TId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        TId.Parse(reader.GetInt64());

    public override void Write(Utf8JsonWriter writer, TId value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.Value);

    public override TId ReadAsPropertyName(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) =>
        reader.GetString() is string s && long.TryParse(s, out var id)
            ? TId.Parse(id)
            : throw new JsonException($"Could not deserialize {typeToConvert.Name}");

    public override void WriteAsPropertyName(Utf8JsonWriter writer, TId value, JsonSerializerOptions options) =>
        writer.WritePropertyName(value.ToString()!);
}

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class GuidTypedIdConverter<TId> : JsonConverter<TId>
    where TId : struct, IRawTypedId<Guid, TId>
{
    private const int MaxEscapedGuidLength = 36;

    public override TId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        TId.Parse(reader.GetGuid());

    public override void Write(Utf8JsonWriter writer, TId value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);

    public override TId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (TryGetGuidCore(ref reader, out var id))
        {
            return TId.Parse(id);
        }

        throw new JsonException($"Could not deserialize {typeToConvert.Name}");
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, TId value, JsonSerializerOptions options) =>
        writer.WritePropertyName(value.ToString()!);

    private bool TryGetGuidCore(ref Utf8JsonReader reader, out Guid value)
    {
        scoped ReadOnlySpan<byte> span;

        if (reader.HasValueSequence)
        {
            long sequenceLength = reader.ValueSequence.Length;
            if (sequenceLength > MaxEscapedGuidLength)
            {
                value = default;
                return false;
            }

            Span<byte> stackSpan = stackalloc byte[MaxEscapedGuidLength];
            reader.ValueSequence.CopyTo(stackSpan);
            span = stackSpan[..(int)sequenceLength];
        }
        else
        {
            if (reader.ValueSpan.Length > MaxEscapedGuidLength)
            {
                value = default;
                return false;
            }

            span = reader.ValueSpan;
        }

        if (span.Length == MaxEscapedGuidLength && Utf8Parser.TryParse(span, out Guid tmp, out _, 'D'))
        {
            value = tmp;
            return true;
        }

        value = default;
        return false;
    }
}
