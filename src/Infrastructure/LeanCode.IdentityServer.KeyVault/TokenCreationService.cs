using System.IdentityModel.Tokens.Jwt;
using IdentityServer4.Configuration;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace LeanCode.IdentityServer.KeyVault;

internal sealed class TokenCreationService : DefaultTokenCreationService
{
    private readonly SigningService signing;

    public TokenCreationService(
        SigningService signing,
#pragma warning disable CS0618 // obsolete
        ISystemClock clock,
#pragma warning restore CS0618 // obsolete
        IKeyMaterialService keys,
        IdentityServerOptions options,
        ILogger<TokenCreationService> logger
    )
        : base(clock, keys, options, logger)
    {
        this.signing = signing;
    }

    protected override Task<string> CreateJwtAsync(JwtSecurityToken jwt)
    {
        return signing.SignTokenAsync(jwt);
    }
}
