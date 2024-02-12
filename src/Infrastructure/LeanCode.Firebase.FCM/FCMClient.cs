using System.Globalization;
using FirebaseAdmin.Messaging;
using LeanCode.Localization.StringLocalizers;

namespace LeanCode.Firebase.FCM;

public class FCMClient<TUserId>
    where TUserId : notnull, IEquatable<TUserId>
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<FCMClient<TUserId>>();

    private readonly FirebaseMessaging messaging;
    private readonly IPushNotificationTokenStore<TUserId> tokenStore;
    private readonly IStringLocalizer stringLocalizer;

    public FCMClient(
        FirebaseMessaging messaging,
        IPushNotificationTokenStore<TUserId> tokenStore,
        IStringLocalizer stringLocalizer
    )
    {
        this.messaging = messaging;
        this.tokenStore = tokenStore;
        this.stringLocalizer = stringLocalizer;
    }

    public virtual LocalizedNotification Localize(CultureInfo culture) => new(stringLocalizer, culture);

    public LocalizedNotification Localize(string lang) => Localize(CultureInfo.GetCultureInfo(lang));

    public Task SendToUserAsync(
        TUserId userId,
        MulticastMessage message,
        CancellationToken cancellationToken = default
    ) => SendToUserAsync(userId, message, false, cancellationToken);

    public Task SendToUsersAsync(
        IReadOnlySet<TUserId> userIds,
        MulticastMessage message,
        CancellationToken cancellationToken = default
    ) => SendToUsersAsync(userIds, message, false, cancellationToken);

    public Task SendAsync(Message message, CancellationToken cancellationToken = default) =>
        SendAsync(message, false, cancellationToken);

    public Task SendMulticastAsync(MulticastMessage message, CancellationToken cancellationToken = default) =>
        SendMulticastAsync(message, false, cancellationToken);

    public Task SendAllAsync(IEnumerable<Message> messages, CancellationToken cancellationToken = default) =>
        SendAllAsync(messages, false, cancellationToken);

    public Task SendAsync(Message message, bool dryRun, CancellationToken cancellationToken = default) =>
        SendAllAsync(new[] { message }, dryRun, cancellationToken);

    public virtual async Task SendToUserAsync(
        TUserId userId,
        MulticastMessage message,
        bool dryRun,
        CancellationToken cancellationToken = default
    )
    {
        message.Tokens = await tokenStore.GetTokensAsync(userId, cancellationToken);

        if (message.Tokens.Count == 0)
        {
            logger.Information("Cannot send push to user {UserId} - no tokens", userId);
        }
        else
        {
            logger.Debug(
                "Sending push notification to user {UserId} that targets {Count} devices",
                userId,
                message.Tokens.Count
            );
            var response = await messaging.SendMulticastAsync(message, dryRun, cancellationToken);
            await HandleBatchResponseAsync(response, message.Tokens, cancellationToken);
        }
    }

    public virtual async Task SendToUsersAsync(
        IReadOnlySet<TUserId> userIds,
        MulticastMessage message,
        bool dryRun,
        CancellationToken cancellationToken = default
    )
    {
        var tokens = await tokenStore.GetTokensAsync(userIds, cancellationToken);
        message.Tokens = tokens.SelectMany(t => t.Value).ToList();

        if (message.Tokens.Count == 0)
        {
            logger.Information("Cannot send push to users {UserIds} - no tokens", userIds);
        }
        else
        {
            logger.Debug(
                "Sending push notification to user {Count} users, targeting {Count} devices",
                userIds.Count,
                message.Tokens.Count
            );
            var response = await messaging.SendMulticastAsync(message, dryRun, cancellationToken);
            await HandleBatchResponseAsync(response, message.Tokens, cancellationToken);
        }
    }

    public virtual async Task SendAllAsync(
        IEnumerable<Message> messages,
        bool dryRun,
        CancellationToken cancellationToken = default
    )
    {
        logger.Debug("Sending {Count} push messages", messages.Count());

        var response = await messaging.SendAllAsync(messages, dryRun, cancellationToken);
        await HandleBatchResponseAsync(response, messages.Select(m => m.Token), cancellationToken);
    }

    public virtual async Task SendMulticastAsync(
        MulticastMessage message,
        bool dryRun,
        CancellationToken cancellationToken = default
    )
    {
        logger.Debug("Sending multicast push message to {Count} targets", message.Tokens.Count);

        var response = await messaging.SendMulticastAsync(message, dryRun, cancellationToken);
        await HandleBatchResponseAsync(response, message.Tokens, cancellationToken);
    }

    private async Task HandleBatchResponseAsync(
        BatchResponse response,
        IEnumerable<string> tokensUsed,
        CancellationToken cancellationToken
    )
    {
        // Remove the leftover tokens
        var tokensToRemove = response
            .Responses.Zip(tokensUsed)
            .Where(ShouldTokenBeRemoved)
            .Select(p => p.Second)
            .ToList();
        if (tokensToRemove.Count != 0)
        {
            logger.Debug("Some PN tokens have to be removed because they either expired or are wrongly configured");
            await tokenStore.RemoveTokensAsync(tokensToRemove, cancellationToken);
            logger.Warning(
                "{Count} tokens removed from token store because of either expired or are wrongly configured",
                tokensToRemove.Count
            );
        }

        // And just throw _something_ in case of error
        if (response.FailureCount > tokensToRemove.Count)
        {
            logger.Warning(
                "There was {Count} failures in sending the push notification",
                response.FailureCount - tokensToRemove.Count
            );
            throw new FCMSendException();
        }
    }

    private static bool ShouldTokenBeRemoved((SendResponse Response, string Token) pair)
    {
        var errorCode = pair.Response.Exception?.MessagingErrorCode;
        return errorCode == MessagingErrorCode.Unregistered || errorCode == MessagingErrorCode.SenderIdMismatch;
    }
}
