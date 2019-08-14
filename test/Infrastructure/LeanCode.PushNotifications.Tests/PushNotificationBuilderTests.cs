using System;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace LeanCode.PushNotifications.Tests
{
    public class PushNotificationBuilderTests
    {
        private readonly IPushNotifications<Guid> pushNotifications;
        private readonly PushNotificationBuilder<Guid> builder;

        public PushNotificationBuilderTests()
        {
            pushNotifications = Substitute.For<IPushNotifications<Guid>>();
            builder = new PushNotificationBuilder<Guid>(pushNotifications);
        }

        [Fact]
        public async Task WithTitle_creates_PN_with_correct_title()
        {
            await builder.WithTitle("Title").SendToAllDevicesAsync();

            _ = pushNotifications
                .Received(1)
                .SendToAllAsync(
                    Guid.Empty,
                    Arg.Is<PushNotification>(pn => pn.Title == "Title"));

            _ = pushNotifications
                .DidNotReceiveWithAnyArgs()
                .SendAsync(default, default, default);
        }

        [Fact]
        public async Task WithContent_creates_PN_with_correct_content()
        {
            await builder.WithContent("Content").SendToAllDevicesAsync();

            _ = pushNotifications
                .Received(1)
                .SendToAllAsync(
                    Guid.Empty,
                    Arg.Is<PushNotification>(pn => pn.Content == "Content"));

            _ = pushNotifications
                .DidNotReceiveWithAnyArgs()
                .SendAsync(default, default, default);
        }

        [Fact]
        public async Task WithData_creates_PN_with_correct_data()
        {
            object data = new object();

            await builder.WithData(data).SendToAllDevicesAsync();

            _ = pushNotifications
                .Received(1)
                .SendToAllAsync(
                    Guid.Empty,
                    Arg.Is<PushNotification>(pn => Equals(pn.Data, data)));

            _ = pushNotifications
                .DidNotReceiveWithAnyArgs()
                .SendAsync(default, default, default);
        }

        [Theory]
        [InlineData(DeviceType.Android)]
        [InlineData(DeviceType.iOS)]
        [InlineData(DeviceType.Chrome)]
        public async Task SendToDevice_sends_PN_to_correct_device_type(DeviceType dt)
        {
            await builder.SendToDeviceAsync(dt);

            _ = pushNotifications
                .Received(1)
                .SendAsync(Guid.Empty, Arg.Is(dt), Arg.Any<PushNotification>());

            _ = pushNotifications
                .DidNotReceiveWithAnyArgs()
                .SendToAllAsync(default, default);
        }
    }
}
