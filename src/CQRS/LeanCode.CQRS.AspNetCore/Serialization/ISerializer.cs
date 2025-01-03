using System.Text.Json;
using LeanCode.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace LeanCode.CQRS.AspNetCore.Serialization;

public interface ISerializer
{
    Task SerializeAsync(Stream utf8Json, object value, Type inputType, CancellationToken cancellationToken);
    ValueTask<object?> DeserializeAsync(Stream utf8Json, Type returnType, CancellationToken cancellationToken);
}

public sealed class Utf8JsonSerializer(IOptions<JsonOptions> options) : ISerializer
{
    public ValueTask<object?> DeserializeAsync(Stream utf8Json, Type returnType, CancellationToken cancellationToken)
    {
        return JsonSerializer.DeserializeAsync(
            utf8Json,
            returnType,
            options.Value.SerializerOptions,
            cancellationToken
        );
    }

    public Task SerializeAsync(Stream utf8Json, object value, Type inputType, CancellationToken cancellationToken)
    {
        return JsonSerializer.SerializeAsync(
            utf8Json,
            value,
            inputType,
            options.Value.SerializerOptions,
            cancellationToken
        );
    }
}
