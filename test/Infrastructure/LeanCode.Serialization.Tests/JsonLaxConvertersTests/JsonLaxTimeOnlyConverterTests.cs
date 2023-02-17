using System.Text.Json;
using Xunit;

namespace LeanCode.Serialization.Tests;

public class JsonLaxTimeOnlyConverterTests
{
    private static readonly JsonSerializerOptions LaxSerializerOptions = new();

    private static readonly TimeOnly SampleTimeOnly = new(12, 34, 56, 115);
    private static readonly TimeOnly SampleTimeOnlyWithGreaterPrecision = new(12, 34, 56, 115, 123);

    static JsonLaxTimeOnlyConverterTests()
    {
        LaxSerializerOptions.Converters.Add(new JsonLaxTimeOnlyConverter());
    }

    [Theory]
    [InlineData("\"12:34:56.115\"")]
    public void Correctly_serializes_and_deserializes_TimeOnly_using_lax_converter(string serialized)
    {
        Assert.Equal(serialized, JsonSerializer.Serialize(SampleTimeOnly, LaxSerializerOptions));
        Assert.Equal(SampleTimeOnly, JsonSerializer.Deserialize<TimeOnly>(serialized, LaxSerializerOptions));
    }

    [Theory]
    [InlineData("\"12:34:56.115\"")]
    public void Correctly_serializes_TimeOnly_with_greater_precision_using_lax_converter(string deserialized)
    {
        Assert.Equal(deserialized, JsonSerializer.Serialize(SampleTimeOnlyWithGreaterPrecision, LaxSerializerOptions));
    }

    [Theory]
    [InlineData("\"213:37:42.123\"")]
    public void Throws_trying_deserialize_incorrect_TimeOnly_using_lax_converter(string serialized)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TimeOnly>(serialized, LaxSerializerOptions));
    }

    [Theory]
    [InlineData("\"12:34:56.1151231\"")]
    public void Correctly_deserializes_TimeOnly_with_greater_precision_using_lax_converter(string serialized)
    {
        Assert.Equal(SampleTimeOnly, JsonSerializer.Deserialize<TimeOnly>(serialized, LaxSerializerOptions));
    }

    [Theory]
    [InlineData("\"12:34:56.115121212121\"")]
    public void Correctly_deserializes_TimeOnly_with_much_greater_precision_using_lax_converter(string serialized)
    {
        Assert.Equal(SampleTimeOnly, JsonSerializer.Deserialize<TimeOnly>(serialized, LaxSerializerOptions));
    }

    [Theory]
    [InlineData("\"2021-12-15T12:34:56.115\"")]
    public void Correctly_deserializes_TimeOnly_with_date_part_using_lax_converter(string serialized)
    {
        Assert.Equal(SampleTimeOnly, JsonSerializer.Deserialize<TimeOnly>(serialized, LaxSerializerOptions));
    }
}
