/*
MIT License

Copyright (c) 2019 Cysharp, Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeanCode.DomainModels.Ulids
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public class UlidJsonConverter : JsonConverter<Ulid>
    {
        /// <summary>
        /// Read a Ulid value represented by a string from JSON.
        /// </summary>
        public override Ulid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                if (reader.TokenType != JsonTokenType.String)
                {
                    throw new JsonException("Expected string");
                }

                if (reader.HasValueSequence)
                {
                    // Parse using ValueSequence
                    var seq = reader.ValueSequence;
                    if (seq.Length != 26)
                    {
                        throw new JsonException("Ulid invalid: length must be 26");
                    }

                    Span<byte> buf = stackalloc byte[26];
                    seq.CopyTo(buf);
                    _ = Ulid.TryParse(buf, out var ulid);
                    return ulid;
                }
                else
                {
                    // Parse usign ValueSpan
                    var buf = reader.ValueSpan;
                    if (buf.Length != 26)
                    {
                        throw new JsonException("Ulid invalid: length must be 26");
                    }

                    _ = Ulid.TryParse(buf, out var ulid);
                    return ulid;
                }
            }
            catch (IndexOutOfRangeException e)
            {
                throw new JsonException("Ulid invalid: length must be 26", e);
            }
            catch (OverflowException e)
            {
                throw new JsonException("Ulid invalid: invalid character", e);
            }
        }

        public override void Write(Utf8JsonWriter writer, Ulid value, JsonSerializerOptions options)
        {
            Span<byte> buf = stackalloc byte[26];
            value.TryWriteStringify(buf);
            writer.WriteStringValue(buf);
        }
    }
}
