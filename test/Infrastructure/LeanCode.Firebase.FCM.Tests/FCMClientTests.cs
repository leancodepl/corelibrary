using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using LeanCode.Localization.StringLocalizers;
using NSubstitute;
using Xunit;

namespace LeanCode.Firebase.FCM.Tests
{
    public class FCMClientTests
    {
        public static readonly string Key = Environment.GetEnvironmentVariable("FCM_KEY");
        public static readonly string Token = Environment.GetEnvironmentVariable("FCM_TOKEN");

        private static readonly Guid UserId = Guid.NewGuid();

        private static readonly FirebaseMessaging Messaging;

        private readonly StubStore store;
        private readonly FCMClient client;

        static FCMClientTests()
        {
            var app = FirebaseConfiguration.Prepare(Key);
            Messaging = FirebaseMessaging.GetMessaging(app);
        }

        public FCMClientTests()
        {
            store = new StubStore(UserId, Token);
            client = new FCMClient(Messaging, store, Substitute.For<IStringLocalizer>());
        }

        [FCMFact]
        public async Task Sends_single_message_to_the_user()
        {
            var message = new MulticastMessage
            {
                Notification = new Notification
                {
                    Title = "Test title",
                    Body = "Test body",
                },
            };

            await client.SendToUserAsync(UserId, message);
        }

        [FCMFact]
        public async Task Sends_single_message_to_multiple_users()
        {
            var message = new MulticastMessage
            {
                Notification = new Notification
                {
                    Title = "Test title",
                    Body = "Test body",
                },
            };

            await client.SendToUsersAsync(new HashSet<Guid> { UserId }, message);
        }

        [FCMFact]
        public async Task Does_nothing_when_user_does_not_have_tokens()
        {
            var message = new MulticastMessage
            {
                Notification = new Notification
                {
                    Title = "Test title",
                    Body = "Test body",
                },
            };

            await client.SendToUserAsync(Guid.NewGuid(), message);
        }

        [FCMFact]
        public async Task Gets_user_tokens_when_sending_the_message()
        {
            var message = new MulticastMessage
            {
                Notification = new Notification
                {
                    Title = "Test title",
                    Body = "Test body",
                },
            };

            await client.SendToUserAsync(UserId, message);

            var token = Assert.Single(message.Tokens);
            Assert.Equal(Token, token);
        }
    }
}
