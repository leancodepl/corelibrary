using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

            client.Send(null).ReturnsForAnyArgs(Task.FromResult<FCMResult>(new FCMSuccess()));

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

            SetToken(token, uid);

            await sender.Send(uid, DeviceType.Android, new PushNotification("", "", null));

            var _ = client.Received(1).Send(Arg.Is<FCMNotification>(n => n.To == token));
        }

        [Fact]
        public async Task Send_converts_the_message_for_Android_correctly()
        {
            var uid = Guid.NewGuid();
            SetToken("token", uid);

            await sender.Send(uid, DeviceType.Android, new PushNotification("", "", null));

            var _ = client.Received(1).Send(Arg.Is<FCMNotification>(n => n.Notification.Sound == "default"));
        }

        [Fact]
        public async Task Send_converts_the_message_for_iOS_correctly()
        {
            var uid = Guid.NewGuid();
            SetToken("token", uid, DeviceType.iOS);

            await sender.Send(uid, DeviceType.iOS, new PushNotification("", "", null));

            var _ = client.Received(1).Send(Arg.Is<FCMNotification>(n => n.Notification.Badge == "1"));
        }

        [Fact]
        public async Task Send_converts_the_message_for_Chrome_correctly()
        {
            var uid = Guid.NewGuid();
            SetToken("token", uid, DeviceType.Chrome);

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
                new PushNotificationToken<Guid>(uid, DeviceType.Android, "a"),
                new PushNotificationToken<Guid>(uid, DeviceType.iOS, "b"),
                new PushNotificationToken<Guid>(uid, DeviceType.Chrome, "c"),
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

        [Fact]
        public async Task Send_updates_token_if_FCMClient_returns_that_it_has_changed()
        {
            const string newToken = "new-token";
            var uid = Guid.NewGuid();

            SetToken("token", uid);
            client.Send(null).ReturnsForAnyArgs(Task.FromResult<FCMResult>(new FCMTokenUpdated(newToken)));

            await sender.Send(uid, DeviceType.Android, new PushNotification("", "", null));

            var _ = provider.Received().UpdateOrAddToken(uid, DeviceType.Android, newToken);
        }

        [Fact]
        public async Task Send_removes_token_if_FCMClient_returns_that_it_is_invalid()
        {
            var uid = Guid.NewGuid();

            SetToken("token", uid);
            client.Send(null).ReturnsForAnyArgs(Task.FromResult<FCMResult>(new FCMInvalidToken()));

            await sender.Send(uid, DeviceType.Android, new PushNotification("", "", null));

            var _ = provider.Received().RemoveInvalidToken(uid, DeviceType.Android);
        }

        [Fact]
        public Task Send_ignores_HTTP_errors()
        {
            return TestSendResult(new FCMHttpError(HttpStatusCode.BadGateway));
        }

        [Fact]
        public Task Send_ignores_other_error()
        {
            return TestSendResult(new FCMOtherError("other-error"));
        }

        [Fact]
        public Task Send_ignores_successful_result()
        {
            return TestSendResult(new FCMSuccess());
        }

        private void SetToken(string token, Guid uid, DeviceType deviceType = DeviceType.Android)
        {
            provider.GetToken(uid, deviceType).Returns(new PushNotificationToken<Guid>(uid, deviceType, token));
        }

        private async Task TestSendResult(FCMResult result)
        {
            var uid = Guid.NewGuid();

            SetToken("token", uid);
            client.Send(null).ReturnsForAnyArgs(Task.FromResult(result));

            await sender.Send(uid, DeviceType.Android, new PushNotification("", "", null));

            var _ = provider.DidNotReceiveWithAnyArgs().UpdateOrAddToken(uid, DeviceType.Android, null);
            _ = provider.DidNotReceiveWithAnyArgs().RemoveInvalidToken(uid, DeviceType.Android);
        }

    }
}
