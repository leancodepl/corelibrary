using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.Serialization
{
    internal class TypedIdConverterAttribute : JsonConverterAttribute
    {
        private static readonly Dictionary<Type, Type> Converters = new Dictionary<Type, Type>
        {
            [typeof(Id<>)] = typeof(IdConverter<>),
            [typeof(IId<>)] = typeof(IIdConverter<>),
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
    }
}
