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
        private FCMResult()
        { }

        public sealed class HttpError : FCMResult
        {
            public HttpStatusCode StatusCode { get; }

            public HttpError(HttpStatusCode statusCode)
            {
                this.StatusCode = statusCode;
            }
        }

        public sealed class OtherError : FCMResult
        {
            public string Error { get; }

            public OtherError(string error)
            {
                this.Error = error;
            }
        }

        public sealed class InvalidToken : FCMResult
        { }

        public sealed class Success : FCMResult
        { }

        public sealed class TokenUpdated : FCMResult
        {
            public string NewToken { get; }

            public TokenUpdated(string newToken)
            {
                this.NewToken = newToken;
            }
        }
    }
}
