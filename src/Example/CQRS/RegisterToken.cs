using System;
using LeanCode.CQRS;
using LeanCode.CQRS.Security;

namespace LeanCode.Example.CQRS
{
    [AllowUnauthorized]
    public class RegisterToken : IRemoteCommand
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }

        public static class ValidationErrors
        {
            public const int InvalidUserId = 10;
        }
    }
}
