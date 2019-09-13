using System;

namespace LeanCode.CQRS.Security.Exceptions
{
    public class InsufficientPermissionException : Exception
    {
        public InsufficientPermissionException(string message)
            : base(message) { }
    }
}
