namespace LeanCode.SmsSender.Exceptions
{
    public class ActionException : ResponseException
    {
        public ActionException(int errorCode, string message)
            : base(errorCode, message)
        { }
    }
}
