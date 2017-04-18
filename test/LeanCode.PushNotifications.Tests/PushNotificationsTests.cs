using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace LeanCode.PushNotifications.Tests
{
    public class PushNotificationsTests
    {
        private readonly IPushNotificationTokenProvider provider;
        private readonly IFCMClient client;

        private readonly PushNotifications<Guid> sender;

        public PushNotificationsTests()
        {
            provider = Substitute.For<IPushNotificationTokenProvider>();
            client = Substitute.For<IFCMClient>();

            provider.GetToken(Guid.Empty, DeviceType.Android).ReturnsForAnyArgs(Task.FromResult<PushNotificationToken<Guid>>(null));
            provider.GetAll(Guid.Empty).ReturnsForAnyArgs(Task.FromResult(new List<PushNotificationToken<Guid>>()));

            sender = new PushNotifications<Guid>(provider, client);
        }

        [Theory]
        [InlineData(DeviceType.Android)]
        [InlineData(DeviceType.iOS)]
        [InlineData(DeviceType.Chrome)]
        public async Task Send_gathers_token_for_device_from_provider(DeviceType deviceType)
        {
            var uid = Guid.NewGuid();

            await sender.Send(uid, deviceType, new PushNotification("", "", null));

            var _ = provider.Received().GetToken(uid, deviceType);
        }

        [Fact]
        public async Task Send_converts_the_message_assigns_token_and_sends_the_notification_using_FCM()
        {
            const string token = "some token";
            var uid = Guid.NewGuid();

            provider.GetToken(uid, DeviceType.Android).Returns(new PushNotificationToken<Guid> { Token = token, DeviceType = DeviceType.Android });

            await sender.Send(uid, DeviceType.Android, new PushNotification("", "", null));

            var _ = client.Received(1).Send(Arg.Is<FCMNotification>(n => n.To == token));
        }

        [Fact]
        public async Task Send_converts_the_message_for_Android_correctly()
        {
            var uid = Guid.NewGuid();
            provider.GetToken(uid, DeviceType.Android).Returns(new PushNotificationToken<Guid> { Token = "token", DeviceType = DeviceType.Android });

            await sender.Send(uid, DeviceType.Android, new PushNotification("", "", null));

            var _ = client.Received(1).Send(Arg.Is<FCMNotification>(n => n.Notification.Sound == "default"));
        }

        [Fact]
        public async Task Send_converts_the_message_for_iOS_correctly()
        {
            var uid = Guid.NewGuid();
            provider.GetToken(uid, DeviceType.iOS).Returns(new PushNotificationToken<Guid> { Token = "token", DeviceType = DeviceType.iOS });

            await sender.Send(uid, DeviceType.iOS, new PushNotification("", "", null));

            var _ = client.Received(1).Send(Arg.Is<FCMNotification>(n => n.Notification.Badge == "1"));
        }

        [Fact]
        public async Task Send_converts_the_message_for_Chrome_correctly()
        {
            var uid = Guid.NewGuid();
            provider.GetToken(uid, DeviceType.Chrome).Returns(new PushNotificationToken<Guid> { Token = "token", DeviceType = DeviceType.Chrome });

            await sender.Send(uid, DeviceType.Chrome, new PushNotification("", "", null));

            var _ = client.Received(1).Send(Arg.Is<FCMNotification>(n => n.Notification.Badge == null && n.Notification.Sound == null));
        }

        [Fact]
        public async Task Send_does_nothing_if_token_does_not_exist()
        {
            var uid = Guid.NewGuid();

            await sender.Send(uid, DeviceType.Android, new PushNotification("", "", null));

            var _ = client.DidNotReceiveWithAnyArgs().Send(null);
        }

        [Fact]
        public async Task SendToAll_gathers_all_tokens_at_once()
        {
            var uid = Guid.NewGuid();

            await sender.SendToAll(uid, new PushNotification("", "", null));

            var _ = provider.Received().GetAll(uid);
        }

        [Fact]
        public async Task SendToAll_sends_separate_notifications_for_each_available_device()
        {
            var uid = Guid.NewGuid();

            provider.GetAll(uid).Returns(Task.FromResult(new List<PushNotificationToken<Guid>>
            {
                new PushNotificationToken<Guid>{ Token = "a", DeviceType = DeviceType.Android },
                new PushNotificationToken<Guid>{ Token = "b", DeviceType = DeviceType.iOS },
                new PushNotificationToken<Guid>{ Token = "c", DeviceType = DeviceType.Chrome },
            }));

            await sender.SendToAll(uid, new PushNotification("", "", null));

            var _ = client.Received(1).Send(Arg.Is<FCMNotification>(n => n.To == "a"));
            _ = client.Received(1).Send(Arg.Is<FCMNotification>(n => n.To == "b"));
            _ = client.Received(1).Send(Arg.Is<FCMNotification>(n => n.To == "c"));
            Assert.Equal(3, client.ReceivedCalls().Count());
        }

        [Fact]
        public async Task SendToAll_does_nothing_if_there_are_no_tokens()
        {
            var uid = Guid.NewGuid();
            await sender.SendToAll(uid, new PushNotification("", "", null));

            var _ = client.DidNotReceiveWithAnyArgs().Send(null);
        }
    }
}
