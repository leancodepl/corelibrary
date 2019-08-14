using System;
using LeanCode.CQRS;
using LeanCode.CQRS.Security;

namespace LeanCode.Example.CQRS
{
    [AllowUnauthorized]
    public class SendNotification : IRemoteCommand
    {
        public Guid UserId { get; set; }
        public string Content { get; set; }
    }
}
