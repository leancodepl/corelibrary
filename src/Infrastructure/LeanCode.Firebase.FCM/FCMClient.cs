using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;

namespace LeanCode.Firebase.FCM
{
    public class FCMClient
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<FCMClient>();

        private readonly FirebaseMessaging messaging;
        private readonly IPushNotificationTokenStore tokenStore;

        public FCMClient(FirebaseMessaging messaging, IPushNotificationTokenStore tokenStore)
        {
            this.messaging = messaging;
            this.tokenStore = tokenStore;
        }

        public Task SendToUserAsync(Guid userId, MulticastMessage message, CancellationToken cancellationToken = default) =>
            SendToUserAsync(userId, message, false, cancellationToken);
        public Task SendAsync(Message message, CancellationToken cancellationToken = default) =>
            SendAsync(message, false, cancellationToken);
        public Task SendMulticastAsync(MulticastMessage message, CancellationToken cancellationToken = default) =>
            SendMulticastAsync(message, false, cancellationToken);
        public Task SendAllAsync(IEnumerable<Message> messages, CancellationToken cancellationToken = default) =>
            SendAllAsync(messages, false, cancellationToken);
        public Task SendAsync(Message message, bool dryRun, CancellationToken cancellationToken = default) =>
            SendAllAsync(new[] { message }, dryRun, cancellationToken);

        public async Task SendToUserAsync(Guid userId, MulticastMessage message, bool dryRun, CancellationToken cancellationToken = default)
        {
            message.Tokens = await tokenStore.GetTokensAsync(userId);

            logger.Debug(
                "Sending push notification to user {UserId} that targets {Count} devices",
                userId, message.Tokens.Count);
            var response = await messaging.SendMulticastAsync(message, dryRun, cancellationToken);
            await HandleBatchResponseAsync(response, message.Tokens);
        }

        public async Task SendAllAsync(IEnumerable<Message> messages, bool dryRun, CancellationToken cancellationToken = default)
        {
            logger.Debug("Sending {Count} push messages", messages.Count());

            var response = await messaging.SendAllAsync(messages, dryRun, cancellationToken);
            await HandleBatchResponseAsync(response, messages.Select(m => m.Token));
        }

        public async Task SendMulticastAsync(MulticastMessage message, bool dryRun, CancellationToken cancellationToken = default)
        {
            logger.Debug("Sending multicast push message to {Count} targets", message.Tokens.Count);

            var response = await messaging.SendMulticastAsync(message, dryRun, cancellationToken);
            await HandleBatchResponseAsync(response, message.Tokens);
        }

        private async Task HandleBatchResponseAsync(BatchResponse response, IEnumerable<string> tokensUsed)
        {
            // Remove the leftover tokens
            var tokensToRemove = response.Responses
                .Zip(tokensUsed)
                .Where(ShouldTokenBeRemoved)
                .Select(p => p.Second)
                .ToList();
            if (tokensToRemove.Any())
            {
                logger.Debug("Some PN tokens have to be removed because they either expired or are wrongly configured");
                await tokenStore.RemoveTokensAsync(tokensToRemove);
                logger.Warning(
                    "{Count} tokens removed from token store because of either expired or are wrongly configured",
                    tokensToRemove.Count);
            }

            // And just throw _something_ in case of error
            if (response.FailureCount > tokensToRemove.Count)
            {
                logger.Warning(
                    "There was {Count} failures in sending the push notification",
                    response.FailureCount - tokensToRemove.Count);
                System.Console.WriteLine(response.Responses[0].Exception.ToString());
                throw new FCMSendException();
            }
        }

        private static bool ShouldTokenBeRemoved((SendResponse response, string token) pair)
        {
            var errorCode = pair.response.Exception?.MessagingErrorCode;
            return errorCode == MessagingErrorCode.Unregistered ||
                errorCode == MessagingErrorCode.SenderIdMismatch;
        }
    }
}
