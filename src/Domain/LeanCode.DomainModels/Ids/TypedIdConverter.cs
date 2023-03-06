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

    public override TId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Read(ref reader, typeToConvert, options);

    public override void WriteAsPropertyName(Utf8JsonWriter writer, TId value, JsonSerializerOptions options) =>
        Write(writer, value, options);
}

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class IntTypedIdConverter<TId> : JsonConverter<TId>
    where TId : struct, IRawTypedId<int, TId>
{
    public override TId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        TId.Parse(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, TId value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.Value);

    public override TId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetString() is string s && int.TryParse(s, out var id)
            ? TId.Parse(id)
            : throw new JsonException($"Could not deserialize {typeToConvert.Name}");

    public override void WriteAsPropertyName(Utf8JsonWriter writer, TId value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString());
}

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class LongTypedIdConverter<TId> : JsonConverter<TId>
    where TId : struct, IRawTypedId<long, TId>
{
    public override TId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        TId.Parse(reader.GetInt64());

    public override void Write(Utf8JsonWriter writer, TId value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.Value);

    public override TId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetString() is string s && long.TryParse(s, out var id)
            ? TId.Parse(id)
            : throw new JsonException($"Could not deserialize {typeToConvert.Name}");

    public override void WriteAsPropertyName(Utf8JsonWriter writer, TId value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString());
}

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class GuidTypedIdConverter<TId> : JsonConverter<TId>
    where TId : struct, IRawTypedId<Guid, TId>
{
    public override TId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        TId.Parse(reader.GetGuid());

    public override void Write(Utf8JsonWriter writer, TId value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);

    public override TId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Read(ref reader, typeToConvert, options);

    public override void WriteAsPropertyName(Utf8JsonWriter writer, TId value, JsonSerializerOptions options) =>
        Write(writer, value, options);
}
