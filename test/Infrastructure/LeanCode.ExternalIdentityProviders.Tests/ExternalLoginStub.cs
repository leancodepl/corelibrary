using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace LeanCode.ExternalIdentityProviders.Tests
{
    internal class ExternalLoginStub : ExternalLoginBase<User>
    {
        public HashSet<string> KnownTokens { get; } = new();
        public override string GrantType => "test";

        public ExternalLoginStub(UserManager<User> userManager)
            : base(userManager)
        { }

        public string AddToken()
        {
            var token = Guid.NewGuid().ToString("N");
            KnownTokens.Add(token);
            return token;
        }

        protected override Task<string?> GetProviderIdAsync(string token)
        {
            return Task.FromResult(KnownTokens.Contains(token) ? token : null);
        }
    }
}
