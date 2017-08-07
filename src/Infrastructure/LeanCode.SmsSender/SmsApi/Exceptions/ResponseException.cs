using System;

namespace LeanCode.SmsSender.SmsApi.Exceptions
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
}