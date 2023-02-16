using System.Text.Json;
using Xunit;

namespace LeanCode.Serialization.Tests;

public class JsonConvertersTests
{
    private static readonly JsonSerializerOptions DefaultSerializerOptions = new();

    private static readonly JsonSerializerOptions LaxSerializerOptions = new();

    static JsonConvertersTests()
    {
        LaxSerializerOptions.Converters.Add(new JsonLaxDateOnlyConverter());
        LaxSerializerOptions.Converters.Add(new JsonLaxTimeOnlyConverter());
        LaxSerializerOptions.Converters.Add(new JsonLaxDateTimeOffsetConverter());
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
    [InlineData("\"20211-12-15\"")]
    public void Throws_trying_deserialize_incorrect_DateOnly(string serialized)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DateOnly>(serialized, DefaultSerializerOptions));
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
    [InlineData(@"""\u0032\u0030\u0032\u0032-\u0030\u0032-\u0030\u0032""", 2022, 2, 2, "\"2022-02-02\"")]
    public void Correctly_deserializes_DateOnly_with_raw_JSON_value_using_lax_converter(
        string serialized, int year, int month, int day, string reserialized)
    {
        var date = new DateOnly(year, month, day);

        Assert.Equal(date, JsonSerializer.Deserialize<DateOnly>(serialized, LaxSerializerOptions));
        Assert.Equal(reserialized, JsonSerializer.Serialize(date, LaxSerializerOptions));
    }

    [Theory]
    [InlineData("\"21:37:42.1230000\"", 21, 37, 42, 123)]
    public void Correctly_serializes_and_deserializes_TimeOnly(string serialized, int hour, int minute, int second, int millisecond)
    {
        var time = new TimeOnly(hour, minute, second, millisecond);

        var a = JsonSerializer.Serialize(time, DefaultSerializerOptions);
        var b = JsonSerializer.Deserialize<TimeOnly>(serialized, DefaultSerializerOptions);

        Assert.Equal(serialized, JsonSerializer.Serialize(time, DefaultSerializerOptions));
        Assert.Equal(time, JsonSerializer.Deserialize<TimeOnly>(serialized, DefaultSerializerOptions));
    }

    [Theory]
    [InlineData("\"213:37:42.123\"")]
    public void Throws_trying_deserialize_incorrect_TimeOnly(string serialized)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TimeOnly>(serialized, DefaultSerializerOptions));
    }

    [Theory]
    [InlineData("\"2021-12-15T07:23:10.115\"", 7, 23, 10, 115, "\"07:23:10.115\"")]
    public void Correctly_deserializes_TimeOnly_with_date_part_when_using_lax_converter(
        string serialized, int hour, int minute, int second, int millisecond, string reserialized)
    {
        var time = new TimeOnly(hour, minute, second, millisecond);

        Assert.Equal(time, JsonSerializer.Deserialize<TimeOnly>(serialized, LaxSerializerOptions));
        Assert.Equal(reserialized, JsonSerializer.Serialize(time, LaxSerializerOptions));
    }

    [Theory]
    [InlineData(@"""\u0030\u0037:\u0032\u0033:\u0031\u0030.\u0031\u0031\u0035""", 7, 23, 10, 115, "\"07:23:10.115\"")]
    public void Correctly_deserializes_TimeOnly_with_raw_JSON_value_using_lax_converter(
        string serialized, int hour, int minute, int second, int millisecond, string reserialized)
    {
        var time = new TimeOnly(hour, minute, second, millisecond);

        Assert.Equal(time, JsonSerializer.Deserialize<TimeOnly>(serialized, LaxSerializerOptions));
        Assert.Equal(reserialized, JsonSerializer.Serialize(time, LaxSerializerOptions));
    }

    [Theory]
    [InlineData("\"2021-12-15T07:23:10.115Z\"", 2021, 12, 15, 7, 23, 10, 115, 0, "\"2021-12-15T07:23:10.115+00:00\"")]
    public void Correctly_deserializes_DateTimeOffset_when_using_lax_converter(
        string serialized, int year, int month, int day, int hour, int minute, int second, int millisecond, int timeSpanOffset, string reserialized)
    {
        var date = new DateTimeOffset(year, month, day, hour, minute, second, millisecond, new TimeSpan(timeSpanOffset, 0, 0));

        Assert.Equal(date, JsonSerializer.Deserialize<DateTimeOffset>(serialized, LaxSerializerOptions));
        Assert.Equal(reserialized, JsonSerializer.Serialize(date, LaxSerializerOptions));
    }

    [Theory]
    [InlineData("\"20212-12-15T07:23:10.115Z\"")]
    public void Throws_trying_deserialize_incorrect_DateTimeOffset(string serialized)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DateTimeOffset>(serialized, DefaultSerializerOptions));
    }

    [Theory]
    [InlineData("\"2021-12-15T07:23:10.115+03:00\"", 2021, 12, 15, 7, 23, 10, 115, 3, "\"2021-12-15T07:23:10.115+03:00\"")]
    public void Correctly_deserializes_DateTimeOffset_with_no_zero_offset_when_using_lax_converter(
        string serialized, int year, int month, int day, int hour, int minute, int second, int millisecond, int timeSpanOffset, string reserialized)
    {
        var date = new DateTimeOffset(year, month, day, hour, minute, second, millisecond, new TimeSpan(timeSpanOffset, 0, 0));

        Assert.Equal(date, JsonSerializer.Deserialize<DateTimeOffset>(serialized, LaxSerializerOptions));
        Assert.Equal(reserialized, JsonSerializer.Serialize(date, LaxSerializerOptions));
    }

    [Theory]
    [InlineData(@"""\u0032\u0030\u0032\u0031-\u0031\u0032-\u0031\u0035T\u0030\u0037:\u0032\u0033:\u0031\u0030.\u0031\u0031\u0035Z""", 2021, 12, 15, 7, 23, 10, 115, 0, "\"2021-12-15T07:23:10.115+00:00\"")]
    public void Correctly_deserializes_DateTimeOffset_with_raw_JSON_value_using_lax_converter(
        string serialized, int year, int month, int day, int hour, int minute, int second, int millisecond, int timeSpanOffset, string reserialized)
    {
        var date = new DateTimeOffset(year, month, day, hour, minute, second, millisecond, new TimeSpan(timeSpanOffset, 0, 0));

        Assert.Equal(date, JsonSerializer.Deserialize<DateTimeOffset>(serialized, LaxSerializerOptions));
        Assert.Equal(reserialized, JsonSerializer.Serialize(date, LaxSerializerOptions));
    }
}
