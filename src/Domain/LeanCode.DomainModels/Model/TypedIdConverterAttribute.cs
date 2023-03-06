using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeanCode.DomainModels.Model;

internal sealed class TypedIdConverterAttribute : JsonConverterAttribute
{
    private static readonly Dictionary<Type, Type> Converters =
        new()
        {
            [typeof(Id<>)] = typeof(IdConverter<>),
            [typeof(IId<>)] = typeof(IIdConverter<>),
            [typeof(LId<>)] = typeof(LIdConverter<>),
            [typeof(SId<>)] = typeof(SIdConverter<>),
        };

    public override JsonConverter CreateConverter(Type typeToConvert)
    {
        if (
            !typeToConvert.IsGenericType
            || !Converters.TryGetValue(typeToConvert.GetGenericTypeDefinition(), out var converterGenericType)
        )
        {
            throw new InvalidOperationException(
                $"{nameof(TypedIdConverterAttribute)} can only by used for strongly typed id types"
            );
        }

        var entityType = typeToConvert.GetGenericArguments()[0];
        var converterType = converterGenericType.MakeGenericType(entityType);
        var converter = Activator.CreateInstance(converterType);
        return (JsonConverter)converter!;
    }

    private class IdConverter<T> : JsonConverter<Id<T>>
        where T : class, IIdentifiable<Id<T>>
    {
        private const int MaxEscapedGuidLength = 36;

        public override Id<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TryGetGuid(out var id))
            {
                return new(id);
            }

            throw new JsonException($"Could not deserialize {typeToConvert.Name}");
        }

        public override void Write(Utf8JsonWriter writer, Id<T> value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }

        public override Id<T> ReadAsPropertyName(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            if (TryGetGuidCore(ref reader, out var id))
            {
                return new(id);
            }

            throw new JsonException($"Could not deserialize {typeToConvert.Name}");
        }

        public override void WriteAsPropertyName(Utf8JsonWriter writer, Id<T> value, JsonSerializerOptions options)
        {
            writer.WritePropertyName(value.ToString());
        }

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

            if (span.Length == MaxEscapedGuidLength
                && Utf8Parser.TryParse(span, out Guid tmp, out _, 'D'))
            {
                value = tmp;
                return true;
            }

            value = default;
            return false;
        }
    }

    private class IIdConverter<T> : JsonConverter<IId<T>>
        where T : class, IIdentifiable<IId<T>>
    {
        public override IId<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TryGetInt32(out var id))
            {
                return new(id);
            }

            throw new JsonException($"Could not deserialize {typeToConvert.Name}");
        }

        public override void Write(Utf8JsonWriter writer, IId<T> value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.Value);
        }

        public override IId<T> ReadAsPropertyName(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            if (reader.GetString() is string s && int.TryParse(s, out var id))
            {
                return new(id);
            }

            throw new JsonException($"Could not deserialize {typeToConvert.Name}");
        }

        public override void WriteAsPropertyName(Utf8JsonWriter writer, IId<T> value, JsonSerializerOptions options)
        {
            writer.WritePropertyName(value.ToString());
        }
    }

    private class LIdConverter<T> : JsonConverter<LId<T>>
        where T : class, IIdentifiable<LId<T>>
    {
        public override LId<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TryGetInt64(out var id))
            {
                return new(id);
            }

            throw new JsonException($"Could not deserialize {typeToConvert.Name}");
        }

        public override void Write(Utf8JsonWriter writer, LId<T> value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.Value);
        }

        public override LId<T> ReadAsPropertyName(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            if (reader.GetString() is string s && long.TryParse(s, out var id))
            {
                return new(id);
            }

            throw new JsonException($"Could not deserialize {typeToConvert.Name}");
        }

        public override void WriteAsPropertyName(Utf8JsonWriter writer, LId<T> value, JsonSerializerOptions options)
        {
            writer.WritePropertyName(value.ToString());
        }
    }

    private class SIdConverter<T> : JsonConverter<SId<T>>
        where T : class, IIdentifiable<SId<T>>
    {
        public override SId<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.GetString() is string s)
            {
                return SId<T>.From(s);
            }

            throw new JsonException($"Could not deserialize {typeToConvert.Name}");
        }

        public override void Write(Utf8JsonWriter writer, SId<T> value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }

        public override SId<T> ReadAsPropertyName(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            return Read(ref reader, typeToConvert, options);
        }

        public override void WriteAsPropertyName(Utf8JsonWriter writer, SId<T> value, JsonSerializerOptions options)
        {
            writer.WritePropertyName(value.Value);
        }
    }
}
