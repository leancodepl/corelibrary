using System;
using LeanCode.CQRS;

namespace LeanCode.Example.CQRS
{
    public class RegisterToken : IRemoteCommand
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }
}
