using System.Text.Json;
using LeanCode.Serialization;

namespace LeanCode.CQRS.AspNetCore.Serialization;

public interface ISerializer
{
    Task SerializeAsync(Stream utf8Json, object value, Type inputType, CancellationToken cancellationToken);
    ValueTask<object?> DeserializeAsync(Stream utf8Json, Type returnType, CancellationToken cancellationToken);
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

    public ValueTask<object?> DeserializeAsync(Stream utf8Json, Type returnType, CancellationToken cancellationToken)
    {
        return JsonSerializer.DeserializeAsync(utf8Json, returnType, options, cancellationToken);
    }

    public Task SerializeAsync(Stream utf8Json, object value, Type inputType, CancellationToken cancellationToken)
    {
        return JsonSerializer.SerializeAsync(utf8Json, value, inputType, options, cancellationToken);
    }
}
