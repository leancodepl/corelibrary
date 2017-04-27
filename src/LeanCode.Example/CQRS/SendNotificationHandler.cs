using System;
using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.PushNotifications;

namespace LeanCode.Example.CQRS
{
    public class SendNotificationHandler : ICommandHandler<SendNotification>
    {
        private readonly IPushNotifications<Guid> pns;

        public SendNotificationHandler(IPushNotifications<Guid> pns)
        {
            this.pns = pns;
        }

        public Task ExecuteAsync(SendNotification command)
        {
            return
                pns.Send(command.UserId, DeviceType.Android, new PushNotification("Notification", command.Content, null));
        }
    }
}
