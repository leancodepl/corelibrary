using System.Text.Json;
using Xunit;

namespace LeanCode.Serialization.Tests;

public class JsonLaxDateTimeOffsetConverterTests
{
    private static readonly JsonSerializerOptions LaxSerializerOptions = new();

    private static readonly DateTimeOffset SampleDateTimeOffsetWithZeroOffset = new(2021, 12, 15, 12, 34, 56, 115, TimeSpan.Zero);
    private static readonly DateTimeOffset SampleDateTimeOffsetWithZeroOffsetAndGreaterPrecision = new(2021, 12, 15, 12, 34, 56, 115, 12, TimeSpan.Zero);
    private static readonly DateTimeOffset SampleDateTimeOffsetWithNonZeroOffset = new(2021, 12, 15, 12, 34, 56, 115, TimeSpan.FromHours(1));

    static JsonLaxDateTimeOffsetConverterTests()
    {
        LaxSerializerOptions.Converters.Add(new JsonLaxDateTimeOffsetConverter());
    }

    [Theory]
    [InlineData("\"2021-12-15T12:34:56.115+00:00\"")]
    public void Correctly_deserialize_and_serialize_DateTimeOffset_with_zero_offset_using_lax_converter(string serialized)
    {
        Assert.Equal(SampleDateTimeOffsetWithZeroOffset, JsonSerializer.Deserialize<DateTimeOffset>(serialized, LaxSerializerOptions));
        Assert.Equal(serialized, JsonSerializer.Serialize(SampleDateTimeOffsetWithZeroOffset, LaxSerializerOptions));
    }

    [Theory]
    [InlineData("\"2021-12-15T12:34:56.115+01:00\"")]
    public void Correctly_deserialize_and_serialize_DateTimeOffset_with_non_zero_offset_using_lax_converter(string serialized)
    {
        Assert.Equal(SampleDateTimeOffsetWithNonZeroOffset, JsonSerializer.Deserialize<DateTimeOffset>(serialized, LaxSerializerOptions));
        Assert.Equal(serialized, JsonSerializer.Serialize(SampleDateTimeOffsetWithNonZeroOffset, LaxSerializerOptions));
    }

    [Theory]
    [InlineData("\"2021-12-15T12:34:56.115+00:00\"")]
    public void Correctly_serialize_DateTimeOffset_with_zero_offset_and_greater_precision_using_lax_converter(string deserialized)
    {
        Assert.Equal(deserialized, JsonSerializer.Serialize(SampleDateTimeOffsetWithZeroOffsetAndGreaterPrecision, LaxSerializerOptions));
    }

    [Theory]
    [InlineData("\"20212-12-15T07:23:10.115Z\"")]
    public void Throws_trying_deserialize_incorrect_DateTimeOffset_using_lax_converter(string serialized)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DateTimeOffset>(serialized, LaxSerializerOptions));
    }

    [Theory]
    [InlineData("\"2021-12-15T12:34:56.1151231+00:00\"")]
    public void Correctly_deserialize_DateTimeOffset_with_zero_offset_and_greater_precision_using_lax_converter(string serialized)
    {
        Assert.Equal(SampleDateTimeOffsetWithZeroOffset, JsonSerializer.Deserialize<DateTimeOffset>(serialized, LaxSerializerOptions));
    }

    [Theory]
    [InlineData("\"2021-12-15T12:34:56.1151231+01:00\"")]
    public void Correctly_deserialize_DateTimeOffset_with_non_zero_offset_and_greater_precision_using_lax_converter(string serialized)
    {
        Assert.Equal(SampleDateTimeOffsetWithNonZeroOffset, JsonSerializer.Deserialize<DateTimeOffset>(serialized, LaxSerializerOptions));
    }

    [Theory]
    [InlineData("\"2021-12-15T12:34:56.11512312311+00:00\"")]
    public void Correctly_deserialize_DateTimeOffset_with_zero_offset_and_much_greater_precision_using_lax_converter(string serialized)
    {
        Assert.Equal(SampleDateTimeOffsetWithZeroOffset, JsonSerializer.Deserialize<DateTimeOffset>(serialized, LaxSerializerOptions));
    }
}
