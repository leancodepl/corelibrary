using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace LeanCode.ExternalIdentityProviders.Google
{
    public class GoogleExternalLogin<TUser> : ExternalLoginBase<TUser>
        where TUser : IdentityUser<Guid>
    {
        private readonly GoogleAuthService googleAuthService;

        public override string GrantType => Constants.GrantType;

        public GoogleExternalLogin(UserManager<TUser> userManager, GoogleAuthService googleAuthService)
            : base(userManager)
        {
            this.googleAuthService = googleAuthService;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1031", Justification = "The method is an exception boundary.")]
        protected override async Task<string?> GetProviderIdAsync(string token)
        {
            try
            {
                if (await googleAuthService.ValidateTokenAsync(token) is GoogleTokenValidationResult.Success success)
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
                Logger.Debug(ex, "Cannot get Google user info");

                return null;
            }
        }
    }
}
