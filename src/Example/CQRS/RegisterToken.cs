using System;
using LeanCode.CQRS;

namespace LeanCode.Example.CQRS
{
    public class RegisterToken : IRemoteCommand<LocalContext>
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }
}
