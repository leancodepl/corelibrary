using System;

namespace LeanCode.ExternalIdentityProviders
{
    public class ExternalLoginException : Exception
    {
        public TokenValidationError? TokenValidation { get; }

        public ExternalLoginException(string? message)
            : base(message)
        { }

        public ExternalLoginException(string? message, TokenValidationError? tokenValidation)
            : base(message)
        {
            TokenValidation = tokenValidation;
        }
    }
}
