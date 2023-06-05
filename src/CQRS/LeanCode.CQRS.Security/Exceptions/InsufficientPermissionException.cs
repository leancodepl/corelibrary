using System;

namespace LeanCode.CQRS.Security.Exceptions;

public class InsufficientPermissionException : Exception
{
    public string? AuthorizerName { get; private set; }

    public InsufficientPermissionException(string message, string? authorizerName)
        : base(message)
    {
        AuthorizerName = authorizerName;
    }
}
