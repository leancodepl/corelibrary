using System;
using System.Threading.Tasks;
using Google.Apis.Auth;

namespace LeanCode.ExternalIdentityProviders.Google
{
    public class GoogleAuthService
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<GoogleAuthService>();
        private readonly GoogleJsonWebSignature.ValidationSettings settings;

        public GoogleAuthService(GoogleAuthConfiguration config)
        {
            settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { config.ClientId },
            };
        }

        public async Task<GoogleTokenValidationResult> ValidateTokenAsync(string idToken)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                var user = new GoogleUser(payload.Subject, payload.Email, payload.EmailVerified);

                return new GoogleTokenValidationResult.Success(user);
            }
            catch (Exception ex)
            {
                logger.Debug(ex, "Cannot get Google user info");

                return new GoogleTokenValidationResult.Failure();
            }
        }
    }
}
