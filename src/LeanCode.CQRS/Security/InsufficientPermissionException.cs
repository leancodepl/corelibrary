using System;

namespace LeanCode.CQRS.Exceptions
{
    public class InsufficientPermissionException : Exception
    {
        public InsufficientPermissionException(string message)
            : base(message)
        { }
    }
}
