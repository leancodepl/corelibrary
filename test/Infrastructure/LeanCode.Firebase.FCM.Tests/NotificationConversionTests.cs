using System.Globalization;
using FluentAssertions;
using Xunit;

namespace LeanCode.Firebase.FCM.Tests;

public class NotificationConversionTests
{
    private readonly NotificationDataConverter converter;

    public NotificationConversionTests()
    {
        converter = new();
        converter.FormatUsing<DateTimeOffset>(d => d.ToString("O", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Converts_int_enum_correctly()
    {
        var data = converter.ToNotificationData(new { Field = IntEnum.Second });

        data.Should().ContainKey("Field").WhoseValue.Should().Be("1");
    }

    [Fact]
    public void Converts_byte_enum_correctly()
    {
        var data = converter.ToNotificationData(new { Field = ByteEnum.Second });

        data.Should().ContainKey("Field").WhoseValue.Should().Be("1");
    }

    [Fact]
    public void Applies_custom_value_formatters()
    {
        var date = new DateTimeOffset(2024, 9, 25, 13, 56, 48, TimeSpan.FromHours(2));
        var data = converter.ToNotificationData(new { Date = date });

        data.Should().ContainKey("Date").WhoseValue.Should().Be("2024-09-25T13:56:48.0000000+02:00");
    }

    private enum IntEnum
    {
        First = 0,
        Second = 1,
    }

    private enum ByteEnum : byte
    {
        First = 0,
        Second = 1,
    }
}
