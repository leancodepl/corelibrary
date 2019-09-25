using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeanCode.Localization.StringLocalizers;
using static LeanCode.PushNotifications.FCMResult;

namespace LeanCode.PushNotifications
{
    public sealed class PushNotifications<TUserId> : IPushNotifications<TUserId>
        where TUserId : notnull
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PushNotifications<TUserId>>();

        private readonly IStringLocalizer stringLocalizer;
        private readonly IPushNotificationTokenStore<TUserId> tokenStore;
        private readonly FCMClient fcmClient;
        private readonly PushNotificationsConfiguration? pushNotificationsConfiguration;

        public PushNotifications(
            IStringLocalizer stringLocalizer,
            IPushNotificationTokenStore<TUserId> tokenStore,
            FCMClient fcmClient,
            PushNotificationsConfiguration? pushNotificationsConfiguration = null)
        {
            this.stringLocalizer = stringLocalizer;
            this.tokenStore = tokenStore;
            this.fcmClient = fcmClient;
            this.pushNotificationsConfiguration = pushNotificationsConfiguration;
        }

        public PushNotificationBuilder<TUserId> New() =>
            new PushNotificationBuilder<TUserId>(this);

        public LocalizedPushNotificationBuilder<TUserId> Localized(string cultureName) =>
            new LocalizedPushNotificationBuilder<TUserId>(cultureName, stringLocalizer, this);

        public async Task SendAsync(TUserId to, DeviceType device, PushNotification notification)
        {
            logger.Verbose("Sending notification to user {UserId} on device {Device}", to, device);

            var token = await tokenStore.GetForDeviceAsync(to, device);

            if (token is null)
            {
                logger.Verbose("User {UserId} does not have tokens for device {Device}", to, device);
            }
            else
            {
                await SendSingleAsync(to, notification, token);
            }
        }

        public async Task SendToAllAsync(TUserId to, PushNotification notification)
        {
            logger.Verbose("Sending notification to user {UserId} on all devices", to);

            var tokens = await tokenStore.GetAllAsync(to);

            if (tokens.Count == 0)
            {
                logger.Verbose("User {UserId} does not have any tokens", to);
            }
            else
            {
                await SendSingleAsync(to, notification, tokens);
            }
        }

        private async Task SendSingleAsync(
            TUserId to,
            PushNotification notification,
            List<PushNotificationToken<TUserId>> tokens)
        {
            var results = await Task.WhenAll(tokens.Select(t => SendAsync(t, notification)));

            // This may touch database, we need to linearize it
            for (int i = 0; i < results.Length; i++)
            {
                await HandleResultAsync(to, tokens[i], results[i]);
            }
        }

        private async Task<FCMResult> SendAsync(PushNotificationToken<TUserId> token, PushNotification notification)
        {
            var converted = NotificationTransformer.Convert(
                token.DeviceType, notification, pushNotificationsConfiguration);

            converted.To = token.Token;

            try
            {
                return await fcmClient.SendAsync(converted);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Cannot send notification to device {DeviceType} - generic error", token.DeviceType);

                return new OtherError("Exception");
            }
        }

        private Task HandleResultAsync(TUserId to, PushNotificationToken<TUserId> token, FCMResult result)
        {
            switch (result)
            {
                case HttpError e:
                    logger.Warning(
                        "Cannot send notification to {UserId} to device {DeviceId}, HTTP status {StatusCode}",
                        to, token.DeviceType, e.StatusCode);

                    break;

                case OtherError e:
                    logger.Warning(
                        "Cannot send notification to {UserId} to device {DeviceId}, FCM error: {FCMError}",
                        to, token.DeviceType, e.Error);

                    break;

                case InvalidToken _:
                    logger.Warning(
                        "Cannot send notification to {UserId} to device {DeviceId}, token is invalid",
                        to, token.DeviceType);

                    return tokenStore.RemoveTokenAsync(token);

                case TokenUpdated e:
                    logger.Information(
                        "Notification to {UserId} to device {DeviceId} sent, updating token with canonical value",
                        to, token.DeviceType);

                    return tokenStore.UpdateTokenAsync(token, e.NewToken);

                case Success _:
                    logger.Information(
                        "Notification to {UserId} to device {DeviceId} sent",
                        to, token.DeviceType);

                    break;
            }

            return Task.CompletedTask;
        }
    }
}
