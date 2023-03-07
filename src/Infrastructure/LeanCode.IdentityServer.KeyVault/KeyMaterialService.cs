using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.IdentityModel.Tokens;

namespace LeanCode.IdentityServer.KeyVault;

internal sealed class KeyMaterialService : IKeyMaterialService
{
    private readonly SigningService signing;

    public KeyMaterialService(SigningService signing)
    {
        this.signing = signing;
    }

    public async Task<IEnumerable<SigningCredentials>> GetAllSigningCredentialsAsync()
    {
        return new[] { await signing.GetSigningCredentialsAsync() };
    }

    public Task<SigningCredentials?> GetSigningCredentialsAsync(IEnumerable<string>? allowedAlgorithms = null)
    {
        var isAllowed = allowedAlgorithms?.Any() ?? false;

        if (!isAllowed || allowedAlgorithms!.Contains(SecurityAlgorithms.RsaSha256))
        {
            return signing.GetSigningCredentialsAsync()!;
        }
        else
        {
            return Task.FromResult<SigningCredentials?>(null);
        }
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
