using System.Collections.Generic;
using System.Threading.Tasks;
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

        public async Task<IEnumerable<SecurityKey>> GetValidationKeysAsync()
        {
            var rsa = await signing.GetKeyAsync();
            return new SecurityKey[] { rsa };
        }
    }
}
