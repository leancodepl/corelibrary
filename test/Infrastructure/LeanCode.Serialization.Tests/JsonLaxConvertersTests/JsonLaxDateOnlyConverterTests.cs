using System.Text.Json;
using Xunit;

namespace LeanCode.Serialization.Tests;

public class JsonLaxDateOnlyConverterTests
{
    private static readonly JsonSerializerOptions LaxSerializerOptions = new();

    private static readonly DateOnly SampleDateOnly = new(2021, 12, 15);

    static JsonLaxDateOnlyConverterTests()
    {
        LaxSerializerOptions.Converters.Add(new JsonLaxDateOnlyConverter());
    }

    [Theory]
    [InlineData("\"2021-12-15\"")]
    public void Correctly_serializes_and_deserializes_DateOnly_using_lax_converter(string serialized)
    {
        Assert.Equal(serialized, JsonSerializer.Serialize(SampleDateOnly, LaxSerializerOptions));
        Assert.Equal(SampleDateOnly, JsonSerializer.Deserialize<DateOnly>(serialized, LaxSerializerOptions));
    }

    [Theory]
    [InlineData("\"20211-12-15\"")]
    public void Throws_trying_deserialize_incorrect_DateOnly_using_lax_converter(string serialized)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DateOnly>(serialized, LaxSerializerOptions));
    }

    [Theory]
    [InlineData("\"2021-12-15T00:00:00\"")]
    public void Correctly_deserializes_DateOnly_with_time_part_using_lax_converter(string serialized)
    {
        Assert.Equal(SampleDateOnly, JsonSerializer.Deserialize<DateOnly>(serialized, LaxSerializerOptions));
    }

    [Theory]
    [InlineData("\"2021-12-15T00:00:00+00:00\"")]
    public void Correctly_deserializes_DateOnly_with_time_part_and_zero_offset_using_lax_converter(string serialized)
    {
        Assert.Equal(SampleDateOnly, JsonSerializer.Deserialize<DateOnly>(serialized, LaxSerializerOptions));
    }

    [Theory]
    [InlineData("\"2021-12-15T00:00:00.000Z\"")]
    public void Correctly_deserializes_DateOnly_with_time_part_and_zero_offset_formated_as_Z_using_lax_converter(
        string serialized
    )
    {
        Assert.Equal(SampleDateOnly, JsonSerializer.Deserialize<DateOnly>(serialized, LaxSerializerOptions));
    }
}
