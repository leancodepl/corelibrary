using FluentAssertions;
using LeanCode.CQRS.AspNetCore.Serialization;

namespace LeanCode.CQRS.AspNetCore.Tests.Middleware;

public sealed class SerializerMock : ISerializer
{
    private const string DefaultContentType = "application/json; charset=utf-8";

    private readonly Dictionary<Type, object?> deserializeResults = [];
    private readonly Dictionary<Type, Exception> deserializeExceptions = [];
    private object? lastSerialized;
    private Type? lastSerializedType;

    public string ContentType => DefaultContentType;

    public Task SerializeAsync(Stream utf8Json, object value, Type inputType, CancellationToken cancellationToken)
    {
        lastSerialized = value;
        lastSerializedType = inputType;
        return Task.CompletedTask;
    }

    public ValueTask<object?> DeserializeAsync(Stream utf8Json, Type returnType, CancellationToken cancellationToken)
    {
        if (deserializeExceptions.TryGetValue(returnType, out var exception))
        {
            throw exception;
        }

        if (deserializeResults.TryGetValue(returnType, out var result))
        {
            return ValueTask.FromResult(result);
        }

        throw new NotSupportedException($"No deserialization result configured for type {returnType}");
    }

    public void SetDeserializeResult<T>(T? result)
    {
        deserializeResults[typeof(T)] = result;
    }

    public void SetDeserializeException<T>(Exception? exception)
    {
        if (exception is not null)
        {
            deserializeExceptions[typeof(T)] = exception;
        }
        else
        {
            deserializeExceptions.Remove(typeof(T));
        }
    }

    public void ShouldHaveSerialized(object expected, Type? expectedType = null)
    {
        lastSerialized.Should().BeSameAs(expected);
        if (expectedType is not null)
        {
            lastSerializedType.Should().Be(expectedType);
        }
    }

    public void ShouldNotHaveSerialized()
    {
        lastSerialized.Should().BeNull();
    }
}
