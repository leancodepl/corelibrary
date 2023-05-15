using System.Text.Json;
using LeanCode.Serialization;

namespace LeanCode.CQRS.AspNetCore.Serialization;

public interface ISerializer
{
    Task SerializeAsync(Stream utf8Json, object value, Type inputType, CancellationToken token);
    ValueTask<object?> DeserializeAsync(Stream utf8Json, Type returnType, CancellationToken token);
}

public sealed class Utf8JsonSerializer : ISerializer
{
    public static readonly JsonSerializerOptions DefaultOptions =
        new()
        {
            Converters =
            {
                new JsonLaxDateOnlyConverter(),
                new JsonLaxTimeOnlyConverter(),
                new JsonLaxDateTimeOffsetConverter(),
            },
        };

    private readonly JsonSerializerOptions? options;

    public Utf8JsonSerializer(JsonSerializerOptions? options)
    {
        this.options = options;
    }

    public Utf8JsonSerializer()
        : this(null) { }

    public ValueTask<object?> DeserializeAsync(Stream utf8Json, Type returnType, CancellationToken token)
    {
        return JsonSerializer.DeserializeAsync(utf8Json, returnType, options, token);
    }

    public Task SerializeAsync(Stream utf8Json, object value, Type inputType, CancellationToken token)
    {
        return JsonSerializer.SerializeAsync(utf8Json, value, inputType, options, token);
    }
}
