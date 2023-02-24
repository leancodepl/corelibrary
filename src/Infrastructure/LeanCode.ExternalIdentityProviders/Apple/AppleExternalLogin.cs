using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace LeanCode.ExternalIdentityProviders.Apple;

public class AppleExternalLogin<TUser> : ExternalLoginBase<TUser>
    where TUser : IdentityUser<Guid>
{
    private readonly AppleIdService appleAuthService;

    public override string GrantType => Constants.GrantType;

    public AppleExternalLogin(UserManager<TUser> userManager, AppleIdService appleAuthService)
        : base(userManager)
    {
        this.appleAuthService = appleAuthService;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "?",
        "CA1031",
        Justification = "The method is an exception boundary."
    )]
    protected override async Task<string?> GetProviderIdAsync(string token)
    {
        try
        {
            if (await appleAuthService.ValidateTokenAsync(token) is AppleTokenValidationResult.Success success)
            {
                return success.User.Id;
            }
            else
            {
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Debug(ex, "Cannot get Apple user info");

            return null;
        }
    }
}
