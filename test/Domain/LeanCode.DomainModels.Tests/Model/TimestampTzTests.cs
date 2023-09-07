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
        AssertionExtensions.Should(TimestampTz.IsValidTimeZoneId(timeZoneId)).BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    public void Invalid_time_zone_ids_are_rejected(string timeZoneId)
    {
        AssertionExtensions.Should(TimestampTz.IsValidTimeZoneId(timeZoneId)).BeFalse();
    }

    [Theory]
    [InlineData("Etc/UTC")]
    [InlineData("Etc/GMT")]
    [InlineData("Europe/Warsaw")]
    public void IANA_time_zone_ids_are_preserved_as_passed(string timeZoneId)
    {
        var timestampTz = new TimestampTz(DateTime.UtcNow, timeZoneId);

        AssertionExtensions.Should(timestampTz.TimeZoneId).Be(timeZoneId);
        AssertionExtensions.Should(timestampTz.TimeZoneInfo).Be(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
    }

    [Theory]
    [InlineData("Etc/UTC")]
    [InlineData("Etc/GMT")]
    [InlineData("Europe/Warsaw")]
    public void IANA_time_zone_infos_are_preserved_as_passed(string timeZoneId)
    {
        var timestampTz = new TimestampTz(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));

        AssertionExtensions.Should(timestampTz.TimeZoneId).Be(timeZoneId);
        AssertionExtensions.Should(timestampTz.TimeZoneInfo).Be(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
    }

    [Fact]
    public void UTC_time_zone_id_is_normalized_to_Etc_UTC()
    {
        var timestampTz = new TimestampTz(DateTime.UtcNow, TimeZoneInfo.Utc.Id);

        AssertionExtensions.Should(timestampTz.TimeZoneId).Be("Etc/UTC");
        AssertionExtensions.Should(timestampTz.TimeZoneInfo).Be(TimeZoneInfo.FindSystemTimeZoneById("Etc/UTC"));
    }

    [Fact]
    public void UTC_time_zone_info_is_normalized_to_Etc_UTC()
    {
        var timestampTz = new TimestampTz(DateTime.UtcNow, TimeZoneInfo.Utc);

        AssertionExtensions.Should(timestampTz.TimeZoneId).Be("Etc/UTC");
        AssertionExtensions.Should(timestampTz.TimeZoneInfo).Be(TimeZoneInfo.FindSystemTimeZoneById("Etc/UTC"));
    }

    [Fact]
    public void Windows_time_zone_ids_are_converted_to_IANA_ids()
    {
        var timestampTz = new TimestampTz(DateTime.UtcNow, "Central European Standard Time");

        AssertionExtensions.Should(timestampTz.TimeZoneId).Be("Europe/Warsaw");
        AssertionExtensions.Should(timestampTz.TimeZoneInfo).Be(TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw"));
    }

    [Fact]
    public void Windows_time_zone_infos_are_converted_to_IANA_ids()
    {
        var timestampTz = new TimestampTz(
            DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time")
        );

        AssertionExtensions.Should(timestampTz.TimeZoneId).Be("Europe/Warsaw");
        AssertionExtensions.Should(timestampTz.TimeZoneInfo).Be(TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    public void Invalid_time_zone_ids_throw_exception(string timeZoneId)
    {
        AssertionExtensions
            .Should(() => new TimestampTz(DateTime.UtcNow, timeZoneId))
            .Throw<TimeZoneNotFoundException>();
    }

    [Fact]
    public void Non_UTC_DateTime_timestamps_throw_exception()
    {
        AssertionExtensions.Should(() => new TimestampTz(DateTime.Now, "Etc/UTC")).Throw<ArgumentException>();
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

        AssertionExtensions.Should(timestampTz.UtcTimestamp).Be(now.UtcDateTime);
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

        AssertionExtensions.Should(timestampTz.LocalTimestampWithOffset).Be(nowInWarsaw);
        AssertionExtensions.Should(timestampTz.LocalTimestampWithoutOffset).Be(nowInWarsaw.DateTime);
    }

    [Fact]
    public void Deconstruction_roundtrips()
    {
        var source = new TimestampTz(DateTime.UtcNow, "Europe/Warsaw");
        var (timestamp, timeZoneId) = source;
        var reconstructed = new TimestampTz(timestamp, timeZoneId);

        AssertionExtensions.Should(reconstructed).Be(source);
    }

    [Fact]
    public void Json_serialization_writes_expected_properties()
    {
        var serialized = JsonSerializer.Serialize(new TimestampTz(DateTime.UtcNow, "Europe/Warsaw"));

        // {"UtcTimestamp":"2009-06-15T13:45:30.0000000+00:00","TimeZoneId":"Europe/Warsaw"}
        using var document = JsonDocument.Parse(serialized);

        AssertionExtensions.Should(document.RootElement.ValueKind).Be(JsonValueKind.Object);
        AssertionExtensions
            .Should(document.RootElement.EnumerateObject())
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

        AssertionExtensions.Should(deserialized).NotBeNull();
        AssertionExtensions.Should(deserialized.TimeZoneId).Be(source.TimeZoneId);
        AssertionExtensions.Should(deserialized.UtcTimestamp).Be(source.UtcTimestamp);
    }
}
