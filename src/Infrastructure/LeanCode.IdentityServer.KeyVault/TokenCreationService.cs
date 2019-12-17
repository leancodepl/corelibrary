using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace LeanCode.IdentityServer.KeyVault
{
    internal class TokenCreationService : DefaultTokenCreationService
    {
        private readonly SigningService signing;

        public TokenCreationService(
            SigningService signing,
            ISystemClock clock,
            IKeyMaterialService keys,
            IdentityServerOptions options,
            ILogger<DefaultTokenCreationService> logger)
            : base(clock, keys, options, logger)
        {
            this.signing = signing;
        }

        protected override Task<string> CreateJwtAsync(JwtSecurityToken jwt)
        {
            return signing.SignTokenAsync(jwt);
        }
    }
}
