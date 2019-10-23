using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS.RemoteHttp.Client
{
    // Why does this class exist, you may ask? Well, it's simple - we don't want to modify
    // `CommandResult` nor `ValidationError` (esp. we don't want to make anything writable in there)
    // and we still want to use System.Text.Json, so we just hand-crafted the parsing logic.
    internal class CommandResultConverter : JsonConverter<CommandResult>
    {
        public override CommandResult Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            IEnumerable<ValidationError>? errors = null;
            while (reader.TokenType != JsonTokenType.EndObject && reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propName = reader.GetString();
                    if (propName == nameof(CommandResult.WasSuccessful))
                    {
                        if (!reader.Read())
                        {
                            throw new JsonException("Cannot deserialize `CommandResult`.");
                        }
                        else if (reader.GetBoolean())
                        {
                            // `WasSuccessful` is set, we don't need to load the errors
                            errors = Enumerable.Empty<ValidationError>();

                            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                            {
                                continue;
                            }

                            break;
                        }
                    }
                    else if (propName == nameof(CommandResult.ValidationErrors))
                    {
                        errors = ReadErrors(ref reader);
                    }
                    else
                    {
                        throw new JsonException($"Cannot deserialize `CommandResult` - unknown property {propName}.");
                    }
                }
                else if (reader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException("Cannot deserialize `CommandResult`.");
                }
            }

            if (errors is null)
            {
                throw new JsonException("Cannot deserialize `CommandResult` - `ValidationErrors` property is missing.");
            }
            else
            {
                return new CommandResult(errors);
            }
        }

        public override void Write(
            Utf8JsonWriter writer,
            CommandResult value,
            JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        private static List<ValidationError> ReadErrors(ref Utf8JsonReader reader)
        {
            if (!reader.Read() || reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Cannot deserialize `CommandResult` - `ValidationErrors` array is malformed.");
            }

            var errors = new List<ValidationError>();

            while (reader.TokenType != JsonTokenType.EndArray && reader.Read())
            {
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    errors.Add(ParseValidationError(ref reader));
                }
                else if (reader.TokenType != JsonTokenType.EndArray)
                {
                    throw new JsonException("Cannot deserialize `CommandResult` - `ValidationErrors` array is malformed.");
                }
            }

            return errors;
        }

        private static ValidationError ParseValidationError(ref Utf8JsonReader reader)
        {
            int? errorCode = null;
            string? propertyName = null;
            string? errorMessage = null;

            while (reader.TokenType != JsonTokenType.EndObject && reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propName = reader.GetString();
                    if (propName == nameof(ValidationError.ErrorCode))
                    {
                        if (!reader.Read() || !reader.TryGetInt32(out var errorCodeTemp))
                        {
                            throw new JsonException("Cannot deserialize `CommandResult` - `ValidationError.ErrorCode` is not a number.");
                        }
                        else
                        {
                            errorCode = errorCodeTemp;
                        }
                    }
                    else if (propName == nameof(ValidationError.ErrorMessage))
                    {
                        errorMessage = ReadStringValue(ref reader);
                    }
                    else if (propName == nameof(ValidationError.PropertyName))
                    {
                        propertyName = ReadStringValue(ref reader);
                    }
                    else
                    {
                        throw new JsonException($"Cannot deserialize `CommandResult` - unknown property {propName} in one of `ValidationError`.");
                    }
                }
                else if (reader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException("Cannot deserialize `CommandResult` - `ValidationErrors` must contain only objects.");
                }
            }

            if (errorCode is int errCode && errorMessage is string errMsg && propertyName is string prop)
            {
                return new ValidationError(prop, errMsg, errCode);
            }
            else
            {
                throw new JsonException("Cannot deserialize `CommandResult` - one of `ValidationError` is malformed.");
            }

            static string ReadStringValue(ref Utf8JsonReader reader)
            {
                if (!reader.Read() || reader.TokenType != JsonTokenType.String)
                {
                    throw new JsonException("Cannot deserialize `CommandResult` - one of `ValidationError` properties is not string.");
                }
                else
                {
                    return reader.GetString();
                }
            }
        }
    }
}
