using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.Serialization
{
    internal sealed class TypedIdConverterAttribute : JsonConverterAttribute
    {
        private static readonly Dictionary<Type, Type> Converters = new()
        {
            [typeof(Id<>)] = typeof(IdConverter<>),
            [typeof(IId<>)] = typeof(IIdConverter<>),
            [typeof(LId<>)] = typeof(LIdConverter<>),
            [typeof(SId<>)] = typeof(SIdConverter<>),
            [typeof(SUlid<>)] = typeof(SUlidConverter<>),
            [typeof(Ulid<>)] = typeof(UlidConverter<>),
        };

        public override JsonConverter CreateConverter(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType
                || !Converters.TryGetValue(typeToConvert.GetGenericTypeDefinition(), out var converterGenericType))
            {
                throw new InvalidOperationException($"{nameof(TypedIdConverterAttribute)} can only by used for strongly typed id types");
            }

            var entityType = typeToConvert.GetGenericArguments()[0];
            var converterType = converterGenericType.MakeGenericType(entityType);
            var converter = Activator.CreateInstance(converterType);
            return (JsonConverter)converter!;
        }

        private class IdConverter<T> : JsonConverter<Id<T>>
            where T : class, IIdentifiable<Id<T>>
        {
            public override Id<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TryGetGuid(out var id))
                {
                    return new Id<T>(id);
                }

                throw new JsonException($"Could not deserialize {typeToConvert.Name}");
            }

            public override void Write(Utf8JsonWriter writer, Id<T> value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.Value);
            }
        }

        private class IIdConverter<T> : JsonConverter<IId<T>>
            where T : class, IIdentifiable<IId<T>>
        {
            public override IId<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TryGetInt32(out var id))
                {
                    return new IId<T>(id);
                }

                throw new JsonException($"Could not deserialize {typeToConvert.Name}");
            }

            public override void Write(Utf8JsonWriter writer, IId<T> value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value.Value);
            }
        }

        private class LIdConverter<T> : JsonConverter<LId<T>>
            where T : class, IIdentifiable<LId<T>>
        {
            public override LId<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TryGetInt64(out var id))
                {
                    return new LId<T>(id);
                }

                throw new JsonException($"Could not deserialize {typeToConvert.Name}");
            }

            public override void Write(Utf8JsonWriter writer, LId<T> value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value.Value);
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
        }

        private class SUlidConverter<T> : JsonConverter<SUlid<T>>
            where T : class, IIdentifiable<SUlid<T>>
        {
            public override bool HandleNull => false;

            public override SUlid<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.GetString() is string s)
                {
                    return SUlid<T>.FromString(s);
                }

                throw new JsonException($"Could not deserialize {typeToConvert.Name}");
            }

            public override void Write(Utf8JsonWriter writer, SUlid<T> value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.Value);
            }
        }

        private class UlidConverter<T> : JsonConverter<Ulid<T>>
            where T : class, IIdentifiable<Ulid<T>>
        {
            public override bool HandleNull => false;

            public override Ulid<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                Span<char> buffer = stackalloc char[64];

                try
                {
                    var count = reader.CopyString(buffer);
                    return new(Ulid.Parse(buffer[..count]));
                }
                catch (Exception e)
                {
                    throw new JsonException("Failed to read input value as Ulid.", e);
                }
            }

            public override void Write(Utf8JsonWriter writer, Ulid<T> value, JsonSerializerOptions options)
            {
                Span<char> buffer = stackalloc char[26];
                var success = value.Value.TryWriteStringify(buffer);
                Debug.Assert(success, "Ulid writing should always succeed.");
                writer.WriteStringValue(buffer);
            }
        }
    }
}
