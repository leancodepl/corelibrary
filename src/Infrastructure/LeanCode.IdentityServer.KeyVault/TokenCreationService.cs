using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using IdentityServer4.Configuration;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace LeanCode.IdentityServer.KeyVault;

internal sealed class TokenCreationService : DefaultTokenCreationService
{
    private readonly SigningService signing;

    [Obsolete]
    public TokenCreationService(
        SigningService signing,
        ISystemClock clock,
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
