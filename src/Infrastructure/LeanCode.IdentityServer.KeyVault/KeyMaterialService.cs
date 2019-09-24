using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.IdentityModel.Tokens;

namespace LeanCode.IdentityServer.KeyVault
{
    internal class KeyMaterialService : IKeyMaterialService
    {
        private readonly SigningService signing;

        public KeyMaterialService(SigningService signing)
        {
            this.signing = signing;
        }

        public Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            return signing.GetSigningCredentialsAsync();
        }

        public async Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            var rsa = await signing.GetKeyAsync();
            return new[]
            {
                new SecurityKeyInfo { Key = rsa, SigningAlgorithm = "RS256" },
            };
        }
    }
}
