using System.Text.Json;
using FluentAssertions;
using LeanCode.DomainModels.Model;
using Xunit;

namespace LeanCode.DomainModels.Tests.Model;

public class TimestampTzTests
{
    [Theory]
    [InlineData("UTC")]
    [InlineData("Etc/UTC")]
    [InlineData("Etc/GMT")]
    [InlineData("Europe/Warsaw")]
    [InlineData("Central European Standard Time")]
    public void Both_IANA_and_Windows_time_zone_ids_are_valid(string timeZoneId)
    {
        TimestampTz.IsValidTimeZoneId(timeZoneId).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    public void Invalid_time_zone_ids_are_rejected(string timeZoneId)
    {
        TimestampTz.IsValidTimeZoneId(timeZoneId).Should().BeFalse();
    }

    [Theory]
    [InlineData("Etc/UTC")]
    [InlineData("Etc/GMT")]
    [InlineData("Europe/Warsaw")]
    public void IANA_time_zone_ids_are_preserved_as_passed(string timeZoneId)
    {
        var timestampTz = new TimestampTz(DateTime.UtcNow, timeZoneId);

        timestampTz.TimeZoneId.Should().Be(timeZoneId);
        timestampTz.TimeZoneInfo.Should().Be(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
    }

    [Theory]
    [InlineData("Etc/UTC")]
    [InlineData("Etc/GMT")]
    [InlineData("Europe/Warsaw")]
    public void IANA_time_zone_infos_are_preserved_as_passed(string timeZoneId)
    {
        var timestampTz = new TimestampTz(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));

        timestampTz.TimeZoneId.Should().Be(timeZoneId);
        timestampTz.TimeZoneInfo.Should().Be(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
    }

    [Fact]
    public void UTC_time_zone_id_is_normalized_to_Etc_UTC()
    {
        var timestampTz = new TimestampTz(DateTime.UtcNow, TimeZoneInfo.Utc.Id);

        timestampTz.TimeZoneId.Should().Be("Etc/UTC");
        timestampTz.TimeZoneInfo.Should().Be(TimeZoneInfo.FindSystemTimeZoneById("Etc/UTC"));
    }

    [Fact]
    public void UTC_time_zone_info_is_normalized_to_Etc_UTC()
    {
        var timestampTz = new TimestampTz(DateTime.UtcNow, TimeZoneInfo.Utc);

        timestampTz.TimeZoneId.Should().Be("Etc/UTC");
        timestampTz.TimeZoneInfo.Should().Be(TimeZoneInfo.FindSystemTimeZoneById("Etc/UTC"));
    }

    [Fact]
    public void Windows_time_zone_ids_are_converted_to_IANA_ids()
    {
        var timestampTz = new TimestampTz(DateTime.UtcNow, "Central European Standard Time");

        timestampTz.TimeZoneId.Should().Be("Europe/Warsaw");
        timestampTz.TimeZoneInfo.Should().Be(TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw"));
    }

    [Fact]
    public void Windows_time_zone_infos_are_converted_to_IANA_ids()
    {
        var timestampTz = new TimestampTz(
            DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time")
        );

        timestampTz.TimeZoneId.Should().Be("Europe/Warsaw");
        timestampTz.TimeZoneInfo.Should().Be(TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    public void Invalid_time_zone_ids_throw_exception(string timeZoneId)
    {
        var act = () => new TimestampTz(DateTime.UtcNow, timeZoneId);
        act.Should().Throw<TimeZoneNotFoundException>();
    }

    [Fact]
    public void Non_UTC_DateTime_timestamps_throw_exception()
    {
        var act = () => new TimestampTz(DateTime.Now, "Etc/UTC");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Non_UTC_DateTimeOffset_timestamps_are_converted_to_UTC(bool useTimeZoneInfo)
    {
        var now = DateTimeOffset.Now;

        var timestampTz = useTimeZoneInfo
            ? new TimestampTz(now, TimeZoneInfo.FindSystemTimeZoneById("Etc/UTC"))
            : new TimestampTz(now, "Etc/UTC");

        timestampTz.UtcTimestamp.Should().Be(now.UtcDateTime);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Local_timestamps_are_correctly_calculated(bool useTimeZoneInfo)
    {
        var now = DateTimeOffset.UtcNow;
        var nowInWarsaw = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(now, "Europe/Warsaw");

        var timestampTz = useTimeZoneInfo
            ? new TimestampTz(now, TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw"))
            : new TimestampTz(now, "Europe/Warsaw");

        timestampTz.LocalTimestampWithOffset.Should().Be(nowInWarsaw);
        timestampTz.LocalTimestampWithoutOffset.Should().Be(nowInWarsaw.DateTime);
    }

    [Fact]
    public void Deconstruction_roundtrips()
    {
        var source = new TimestampTz(DateTime.UtcNow, "Europe/Warsaw");
        var (timestamp, timeZoneId) = source;
        var reconstructed = new TimestampTz(timestamp, timeZoneId);

        reconstructed.Should().Be(source);
    }

    [Fact]
    public void Json_serialization_writes_expected_properties()
    {
        var serialized = JsonSerializer.Serialize(new TimestampTz(DateTime.UtcNow, "Europe/Warsaw"));

        // {"UtcTimestamp":"2009-06-15T13:45:30.0000000+00:00","TimeZoneId":"Europe/Warsaw"}
        using var document = JsonDocument.Parse(serialized);

        document.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
        document.RootElement
            .EnumerateObject()
            .Should()
            .BeEquivalentTo(
                [
                    new { Name = nameof(TimestampTz.UtcTimestamp), Value = new { ValueKind = JsonValueKind.String } },
                    new { Name = nameof(TimestampTz.TimeZoneId), Value = new { ValueKind = JsonValueKind.String } },
                ],
                options => options.WithStrictOrdering()
            );
    }

    [Fact]
    public void Json_serialization_roundtrips()
    {
        var source = new TimestampTz(DateTime.UtcNow, "Europe/Warsaw");
        var serialized = JsonSerializer.Serialize(source);
        var deserialized = JsonSerializer.Deserialize<TimestampTz>(serialized)!;

        deserialized.Should().NotBeNull();
        deserialized.TimeZoneId.Should().Be(source.TimeZoneId);
        deserialized.UtcTimestamp.Should().Be(source.UtcTimestamp);
    }
}
