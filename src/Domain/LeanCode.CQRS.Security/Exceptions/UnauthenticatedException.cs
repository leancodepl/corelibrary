using System;

namespace LeanCode.CQRS.Security.Exceptions
{
    public class UnauthenticatedException : Exception
    {
        public UnauthenticatedException(string message)
            : base(message) { }
    }
}
