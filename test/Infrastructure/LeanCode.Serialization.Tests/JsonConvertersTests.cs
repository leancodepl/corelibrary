using System.Text.Json;
using Xunit;

namespace LeanCode.Serialization.Tests;

public class JsonConvertersTests
{
    private static readonly JsonSerializerOptions DefaultSerializerOptions = new();

    private static readonly JsonSerializerOptions LaxSerializerOptions = new();

    static JsonConvertersTests()
    {
        DefaultSerializerOptions.Converters.Add(new JsonDateOnlyConverter());
        DefaultSerializerOptions.Converters.Add(new JsonTimeOnlyConverter());

        LaxSerializerOptions.Converters.Add(new JsonLaxDateOnlyConverter());
    }

    [Theory]
    [InlineData("\"2021-12-15T00:00:00Z\"")]
    public void Throws_when_attempting_to_deserialize_DateOnly_with_time_part(string serialized)
    {
        Assert.Throws<FormatException>(() => JsonSerializer.Deserialize<DateOnly>(serialized, DefaultSerializerOptions));
    }

    [Theory]
    [InlineData("\"2021-12-15T00:00:00Z\"", 2021, 12, 15, "\"2021-12-15\"")]
    public void Correctly_deserializes_DateOnly_with_time_part_when_using_lax_converter(
        string serialized, int year, int month, int day, string reserialized)
    {
        var date = new DateOnly(year, month, day);

        Assert.Equal(date, JsonSerializer.Deserialize<DateOnly>(serialized, LaxSerializerOptions));
        Assert.Equal(reserialized, JsonSerializer.Serialize(date, LaxSerializerOptions));
    }

    [Theory]
    [InlineData("\"2021-12-15\"", 2021, 12, 15)]
    public void Correctly_serializes_and_deserializes_DateOnly(string serialized, int year, int month, int day)
    {
        var date = new DateOnly(year, month, day);

        Assert.Equal(serialized, JsonSerializer.Serialize(date, DefaultSerializerOptions));
        Assert.Equal(date, JsonSerializer.Deserialize<DateOnly>(serialized, DefaultSerializerOptions));
    }

    [Theory]
    [InlineData("\"21:37:42.1230000\"", 21, 37, 42, 123)]
    public void Correctly_serializes_and_deserializes_TimeOnly(string serialized, int hour, int minute, int second, int millisecond)
    {
        var time = new TimeOnly(hour, minute, second, millisecond);

        Assert.Equal(serialized, JsonSerializer.Serialize(time, DefaultSerializerOptions));
        Assert.Equal(time, JsonSerializer.Deserialize<TimeOnly>(serialized, DefaultSerializerOptions));
    }
}
