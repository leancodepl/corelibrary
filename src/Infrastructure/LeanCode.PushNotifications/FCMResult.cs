using System.Net;

namespace LeanCode.PushNotifications
{
    public abstract class FCMResult
    {
        private FCMResult() { }

        public sealed class HttpError : FCMResult
        {
            public HttpStatusCode StatusCode { get; }

            public HttpError(HttpStatusCode statusCode)
            {
                StatusCode = statusCode;
            }
        }

        public sealed class OtherError : FCMResult
        {
            public string Error { get; }

            public OtherError(string error)
            {
                Error = error;
            }
        }

        public sealed class InvalidToken : FCMResult { }

        public sealed class Success : FCMResult { }

        public sealed class TokenUpdated : FCMResult
        {
            public string NewToken { get; }

            public TokenUpdated(string newToken)
            {
                NewToken = newToken;
            }
        }
    }
}
