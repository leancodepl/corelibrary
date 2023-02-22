using Xunit;

namespace LeanCode.Firebase.FCM.Tests;

public class NotificationConversionTests
{
    public NotificationConversionTests() { }

    [Fact]
    public void Converts_int_enum_correctly()
    {
        var data = Notifications.ToNotificationData(new { Field = IntEnum.Second });

        Assert.Same("1", data["Field"]);
    }

    [Fact]
    public void Converts_byte_enum_correctly()
    {
        var data = Notifications.ToNotificationData(new { Field = ByteEnum.Second });

        Assert.Same("1", data["Field"]);
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
