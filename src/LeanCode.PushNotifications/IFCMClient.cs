using System.Net;
using System.Threading.Tasks;

namespace LeanCode.PushNotifications
{
    public interface IFCMClient
    {
        Task<FCMResult> Send(FCMNotification notification);
    }

    public abstract class FCMResult
    {

    }

    public sealed class FCMHttpError : FCMResult
    {
        public HttpStatusCode StatusCode { get; }

        public FCMHttpError(HttpStatusCode statusCode)
        {
            this.StatusCode = statusCode;
        }
    }

    public sealed class FCMOtherError : FCMResult
    {
        public string Error { get; }

        public FCMOtherError(string error)
        {
            this.Error = error;
        }
    }

    public sealed class FCMInvalidToken : FCMResult
    { }

    public sealed class FCMSuccess : FCMResult
    { }

    public sealed class FCMTokenUpdated : FCMResult
    {
        public string NewToken { get; }

        public FCMTokenUpdated(string newToken)
        {
            this.NewToken = newToken;
        }
    }

}
