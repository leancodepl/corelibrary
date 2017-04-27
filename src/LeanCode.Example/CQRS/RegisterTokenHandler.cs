using System;
using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.PushNotifications;

namespace LeanCode.Example.CQRS
{
    public class RegisterTokenHandler : ICommandHandler<RegisterToken>
    {
        private readonly IPushNotificationTokenStore<Guid> store;

        public RegisterTokenHandler(IPushNotificationTokenStore<Guid> store)
        {
            this.store = store;
        }

        public Task ExecuteAsync(RegisterToken command)
        {
            return store.UpdateOrAddToken(command.UserId, DeviceType.Android, command.Token);
        }
    }
}
