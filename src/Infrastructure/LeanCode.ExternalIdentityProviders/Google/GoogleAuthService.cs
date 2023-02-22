using System;
using System.Threading.Tasks;
using Google.Apis.Auth;

namespace LeanCode.ExternalIdentityProviders.Google;

public class GoogleAuthService
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<GoogleAuthService>();
    private readonly GoogleJsonWebSignature.ValidationSettings settings;

    public GoogleAuthService(GoogleAuthConfiguration config)
    {
        settings = new() { Audience = config.ClientIds, };
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "?",
        "CA1031",
        Justification = "The method is an exception boundary."
    )]
    public async Task<GoogleTokenValidationResult> ValidateTokenAsync(string idToken)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new GoogleTokenValidationResult.Success(
                new(payload.Subject, payload.Email, payload.EmailVerified, payload.Picture)
            );
        }
        catch (Exception ex)
        {
            logger.Debug(ex, "Cannot get Google user info");

            return new GoogleTokenValidationResult.Failure();
        }
    }
}
