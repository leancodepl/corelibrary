using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using IdentityModel;
using Microsoft.IdentityModel.Tokens;

namespace LeanCode.IdentityServer.KeyVault
{
    internal class SigningService
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<SigningService>();

        private readonly CryptographyClient cryptoClient;
        private readonly KeyClient keyClient;

        private RsaSecurityKey? key;

        public SigningService(KeyClient keyClient, CryptographyClient cryptoClient)
        {
            this.keyClient = keyClient;
            this.cryptoClient = cryptoClient;
        }

        public ValueTask<RsaSecurityKey> GetKeyAsync()
        {
            if (key is null)
            {
                return new ValueTask<RsaSecurityKey>(DownloadKeyAsync());
            }
            else
            {
                return new ValueTask<RsaSecurityKey>(key);
            }
        }

        public async Task<SigningCredentials> GetSigningCredentialsAsync() =>
            new SigningCredentials(await GetKeyAsync(), SecurityAlgorithms.RsaSha256);

        public async Task<string> SignTokenAsync(JwtSecurityToken jwt)
        {
            var hp = $"{jwt.EncodedHeader}.{jwt.EncodedPayload}";

            var rawBytes = Encoding.UTF8.GetBytes(hp);
            var digest = SHA256.HashData(rawBytes);

            var signatureRaw = await cryptoClient.SignAsync(SignatureAlgorithm.RS256, digest);

            var signature = Base64Url.Encode(signatureRaw.Signature);

            return $"{hp}.{signature}";
        }

        private async Task<RsaSecurityKey> DownloadKeyAsync()
        {
            logger.Information("Downloading signing key");

            try
            {
                var keyResult = await keyClient.GetKeyAsync(cryptoClient.KeyId);
                return key = new RsaSecurityKey(keyResult.Value.Key.ToRSA());
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Cannot get signing key");

                throw;
            }
        }
    }
}
