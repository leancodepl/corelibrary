using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace LeanCode.ExternalIdentityProviders.Apple
{
    public class AppleIdService
    {
        private const string KeysCacheKey = "apple-keys";
        private static readonly TimeSpan KeysCacheTime = TimeSpan.FromHours(24);

        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<AppleIdService>();

        private readonly HttpClient httpClient;
        private readonly AppleIdConfiguration config;
        private readonly IMemoryCache memoryCache;

        private readonly CryptoProviderFactory cryptoProviderFactory;
        private readonly JwtSecurityTokenHandler tokenHandler;

        public AppleIdService(HttpClient httpClient, AppleIdConfiguration config, IMemoryCache memoryCache)
        {
            this.httpClient = httpClient;
            this.config = config;
            this.memoryCache = memoryCache;

            // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/issues/1302
            cryptoProviderFactory = new() { CacheSignatureProviders = false };
            tokenHandler = new();
        }

        public async Task<AppleTokenValidationResult> ValidateTokenAsync(string idToken)
        {
            var keySet = await GetAppleKeySetAsync();

            var tokenParams = new TokenValidationParameters
            {
                CryptoProviderFactory = cryptoProviderFactory,
                IssuerSigningKeys = keySet.Keys,
                ValidIssuer = "https://appleid.apple.com",
                ValidAudience = config.ClientId,
            };

            try
            {
                var token = tokenHandler.ValidateToken(idToken, tokenParams, out var _);

                var uid = token.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(uid))
                {
                    logger.Warning(
                        "Cannot validate AppleId token {Token} - it does not contain sub claim",
                        idToken);

                    return new AppleTokenValidationResult.Failure();
                }
                else
                {
                    var email = token.FindFirst("email")?.Value;
                    var emailConfirmed = token.FindFirst("email_verified")?.Value == "true";
                    var picture = token.FindFirst("picture")?.Value;

                    return new AppleTokenValidationResult.Success(new(uid, email, emailConfirmed, picture));
                }
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "AppleID token {Token} could not be validated", idToken);

                return new AppleTokenValidationResult.Failure();
            }
        }

        private Task<JsonWebKeySet> GetAppleKeySetAsync()
        {
            return memoryCache.GetOrCreateAsync(
                KeysCacheKey,
                async entry =>
                {
                    entry.SetAbsoluteExpiration(KeysCacheTime);

                    logger.Debug("Downloading Apple signing keys");

                    return JsonWebKeySet.Create(await httpClient.GetStringAsync("https://appleid.apple.com/auth/keys"));
                });
        }
    }
}
