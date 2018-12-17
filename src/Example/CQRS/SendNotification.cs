using System;
using LeanCode.CQRS;

namespace LeanCode.Example.CQRS
{
    public class SendNotification : IRemoteCommand
    {
        public Guid UserId { get; set; }
        public string Content { get; set; }
    }
}
