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
                try
                {
                    await Send(token, notification).ConfigureAwait(false);
                    logger.Information("Notification to user {UserId} on device {Device} sent successfully", to, device);
                }
                catch (Exception e)
                {
                    logger.Warning(e, "Cannot send notification to user {UserId} on device {Device}", to, device);
                }
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
                var tasks = tokens.Select(t => Send(t, notification));
                try
                {
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                    logger.Information("Notification to user {UserId} sent to devices {Devices}", tokens.Select(t => t.DeviceType));
                }
                catch (Exception e)
                {
                    logger.Warning(e, "Cannot send notification to user {UserId}", to);
                }
            }
            else
            {
                logger.Verbose("User {UserId} does not have any tokens", to);
            }
        }

        private Task Send(PushNotificationToken<TUserId> token, PushNotification notification)
        {
            var converted = NotificationTransformer.Convert(token.DeviceType, notification);
            converted.To = token.Token;
            return fcmClient.Send(converted);
        }
    }
}
