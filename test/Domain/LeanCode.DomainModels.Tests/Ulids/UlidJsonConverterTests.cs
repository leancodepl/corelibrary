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


using System.Text.Json;
using FluentAssertions;
using LeanCode.DomainModels.Ulids;
using Xunit;

namespace LeanCode.DomainModels.Tests.Ulids;

public class UlidJsonConverterTest
{
    private sealed class TestSerializationClass
    {
        public Ulid Value { get; set; }
    }

    private static JsonSerializerOptions GetOptions()
    {
        return new JsonSerializerOptions() { Converters = { new UlidJsonConverter() } };
    }

    [Fact]
    public void DeserializeTest()
    {
        var target = Ulid.NewUlid();
        var src = $"{{\"Value\": \"{target.ToString()}\"}}";

        var parsed = JsonSerializer.Deserialize<TestSerializationClass>(src, GetOptions())!;
        parsed.Value.Should().BeEquivalentTo(target, "JSON deserialization should parse string as Ulid");
    }

    [Fact]
    public void DeserializeExceptionTest()
    {
        var target = Ulid.NewUlid();
        var src = $"{{\"Value\": \"{target.ToString().Substring(1)}\"}}";

        var deserializeAction = () => JsonSerializer.Deserialize<TestSerializationClass>(src, GetOptions());
        deserializeAction.Should().Throw<JsonException>();
    }

    [Fact]
    public void SerializeTest()
    {
        var groundTruth = new TestSerializationClass { Value = Ulid.NewUlid() };

        var serialized = JsonSerializer.Serialize(groundTruth, GetOptions());
        var deserialized = JsonSerializer.Deserialize<TestSerializationClass>(serialized, GetOptions())!;
        deserialized.Value.Should().BeEquivalentTo(groundTruth.Value, "JSON serialize roundtrip");
    }

    [Fact]
    public void WithoutOptionsTest()
    {
        var groundTruth = new TestSerializationClass { Value = Ulid.NewUlid() };

        var serialized = JsonSerializer.Serialize(groundTruth);
        var deserialized = JsonSerializer.Deserialize<TestSerializationClass>(serialized)!;
        deserialized.Value.Should().BeEquivalentTo(groundTruth.Value, "JSON serialize roundtrip");
    }
}
