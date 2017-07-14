using System;

namespace LeanCode.Facebook
{
    public class FacebookException : Exception
    {
        public FacebookException(string msg)
            : base(msg)
        { }

        public FacebookException(string msg, Exception innerException)
            : base(msg, innerException)
        { }
    }
}
