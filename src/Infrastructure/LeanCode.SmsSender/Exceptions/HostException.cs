namespace LeanCode.SmsSender.Exceptions
{
    public class HostException : ResponseException
    {
        public HostException(int errorCode, string message)
            : base(errorCode, message)
        { }
    }
}