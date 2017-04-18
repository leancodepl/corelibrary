using System.Threading.Tasks;

namespace LeanCode.PushNotifications
{
    public interface IFCMClient
    {
        Task Send(FCMNotification notification);
    }
}
