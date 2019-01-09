using System;
using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;
using LeanCode.PushNotifications;

namespace LeanCode.Example.CQRS
{
    public class SendNotificationHandler : ICommandHandler<AppContext, SendNotification>
    {
        private readonly IPushNotifications<Guid> pns;

        public SendNotificationHandler(IPushNotifications<Guid> pns)
        {
            this.pns = pns;
        }

        public Task ExecuteAsync(AppContext _, SendNotification command)
        {
            return pns.Send(
                command.UserId, DeviceType.Android,
                new PushNotification("Notification", command.Content, null)
            );
        }
    }
}
