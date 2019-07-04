using System;
using System.Threading.Tasks;
using LeanCode.Localization;
using LeanCode.Localization.StringLocalizers;
using NSubstitute;
using Xunit;

namespace LeanCode.PushNotifications.Tests
{
    public class LocalizedPushNotificationBuilderTests
    {
        private readonly IPushNotifications<Guid> pushNotifications;
        private readonly LocalizedPushNotificationBuilder<Guid> builder;

        protected virtual string Simple => "Order";
        protected virtual string Formatted(int n) => $"Order no. {n}";

        public LocalizedPushNotificationBuilderTests()
            : this("") { }

        protected LocalizedPushNotificationBuilderTests(string cultureName)
        {
            var stringLocalizer = new ResourceManagerStringLocalizer(
                new LocalizationConfiguration(
                    resourceSource: typeof(LocalizedPushNotificationBuilderTests)));

            pushNotifications = Substitute.For<IPushNotifications<Guid>>();

            builder = new LocalizedPushNotificationBuilder<Guid>(
                cultureName, stringLocalizer, pushNotifications);
        }

        [Fact]
        public async Task WithTitle_creates_PN_with_correct_title()
        {
            await builder.WithTitle("order.simple").SendToAllDevicesAsync();

            _ = pushNotifications
                .Received(1)
                .SendToAllAsync(Guid.Empty,
                    Arg.Is<PushNotification>(pn => pn.Title == Simple));

            _ = pushNotifications
                .DidNotReceiveWithAnyArgs()
                .SendAsync(default, default, default);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(42)]
        [InlineData(9001)]
        public async Task WithTitle_creates_PN_with_correct_formatted_title(int n)
        {
            await builder.WithTitle("order.format", n).SendToAllDevicesAsync();

            _ = pushNotifications
                .Received(1)
                .SendToAllAsync(Guid.Empty,
                    Arg.Is<PushNotification>(pn => pn.Title == Formatted(n)));

            _ = pushNotifications
                .DidNotReceiveWithAnyArgs()
                .SendAsync(default, default, default);
        }

        [Fact]
        public async Task WithContent_creates_PN_with_correct_content()
        {
            await builder.WithContent("order.simple").SendToAllDevicesAsync();

            _ = pushNotifications
                .Received(1)
                .SendToAllAsync(Guid.Empty,
                    Arg.Is<PushNotification>(pn => pn.Content == Simple));

            _ = pushNotifications
                .DidNotReceiveWithAnyArgs()
                .SendAsync(default, default, default);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(42)]
        [InlineData(9001)]
        public async Task WithContent_creates_PN_with_correct_formatted_content(int n)
        {
            await builder.WithContent("order.format", n).SendToAllDevicesAsync();

            _ = pushNotifications
                .Received(1)
                .SendToAllAsync(Guid.Empty,
                    Arg.Is<PushNotification>(pn => pn.Content == Formatted(n)));

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
                .SendToAllAsync(Guid.Empty,
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

    public class LocalizedPushNotificationBuilderTestsPL
        : LocalizedPushNotificationBuilderTests
    {
        protected override string Simple => "Zamówienie";
        protected override string Formatted(int n) => $"Zamówienie nr {n}";

        public LocalizedPushNotificationBuilderTestsPL()
            : base("pl") { }
    }
}
