using System;
using System.Linq;
using System.Threading.Tasks;

namespace LeanCode.PushNotifications
{
    public sealed class PushNotifications<TUserId> : IPushNotifications<TUserId>
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PushNotifications<TUserId>>();

        private readonly IPushNotificationTokenProvider<TUserId> tokenProvider;
        private readonly IFCMClient fcmClient;

        public PushNotifications(
            IPushNotificationTokenProvider<TUserId> tokenProvider,
            IFCMClient fcmClient)
        {
            this.tokenProvider = tokenProvider;
            this.fcmClient = fcmClient;
        }

        public async Task Send(TUserId to, DeviceType device, PushNotification notification)
        {
            logger.Verbose("Sending notification to user {UserId} on device {Device}", to, device);

            var token = await tokenProvider.GetToken(to, device).ConfigureAwait(false);
            if (token != null)
            {
                var result = await Send(token, notification).ConfigureAwait(false);
                await HandleResult(to, token, result).ConfigureAwait(false);
            }
            else
            {
                logger.Verbose("User {UserId} does not have token for device {Device}", to, device);
            }
        }

        public async Task SendToAll(TUserId to, PushNotification notification)
        {
            logger.Verbose("Sending notification to user {UserId} on all devices", to);

            var tokens = await tokenProvider.GetAll(to).ConfigureAwait(false);
            if (tokens.Count > 0)
            {
                var results = await Task.WhenAll(tokens.Select(t => Send(t, notification))).ConfigureAwait(false);
                // This may touch database, we need to linearize it
                for (int i = 0; i < results.Length; i++)
                {
                    await HandleResult(to, tokens[i], results[i]).ConfigureAwait(false);
                }
            }
            else
            {
                logger.Verbose("User {UserId} does not have any tokens", to);
            }
        }

        private async Task<FCMResult> Send(PushNotificationToken<TUserId> token, PushNotification notification)
        {
            var converted = NotificationTransformer.Convert(token.DeviceType, notification);
            converted.To = token.Token;
            try
            {
                return await fcmClient.Send(converted);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Cannot send notification to device {DeviceType} - generic error", token.DeviceType);
                return new FCMOtherError("Exception");
            }
        }

        private Task HandleResult(TUserId to, PushNotificationToken<TUserId> token, FCMResult result)
        {
            switch (result)
            {
                case FCMHttpError e:
                    logger.Warning("Cannot send notification to {UserId} to device {DeviceId}, HTTP status {StatusCode}", to, token.DeviceType, e.StatusCode);
                    break;

                case FCMOtherError e:
                    logger.Warning("Cannot send notification to {UserId} to device {DeviceId}, FCM error: {FCMError}", to, token.DeviceType, e.Error);
                    break;

                case FCMInvalidToken e:
                    logger.Warning("Cannot send notification to {UserId} to device {DeviceId}, token is invalid", to, token.DeviceType);
                    return tokenProvider.RemoveInvalidToken(to, token.DeviceType);

                case FCMTokenUpdated e:
                    logger.Information("Notification to {UserId} to device {DeviceId} sent, updating token with canonical value", to, token.DeviceType);
                    return tokenProvider.UpdateOrAddToken(to, token.DeviceType, e.NewToken);

                case FCMSuccess e:
                    logger.Information("Notification to {UserId} to device {DeviceId} sent", to, token.DeviceType);
                    break;
            }
            return Task.CompletedTask;
        }
    }
}
