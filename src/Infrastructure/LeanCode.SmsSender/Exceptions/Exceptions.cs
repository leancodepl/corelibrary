using System;

namespace LeanCode.SmsSender.Exceptions
{
    public abstract class ResponseException : Exception
    {
        public int ErrorCode { get; private set; }

        protected ResponseException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    public class ActionException : ResponseException
    {
        public ActionException(int errorCode, string message)
            : base(errorCode, message) { }
    }

    public class ClientException : ResponseException
    {
        public ClientException(int errorCode, string message)
            : base(errorCode, message) { }
    }

    public class HostException : ResponseException
    {
        public HostException(int errorCode, string message)
            : base(errorCode, message) { }
    }
}
