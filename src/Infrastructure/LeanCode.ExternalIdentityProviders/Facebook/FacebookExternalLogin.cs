using System;
using System.Threading.Tasks;
using LeanCode.Facebook;
using Microsoft.AspNetCore.Identity;

namespace LeanCode.ExternalIdentityProviders.Facebook
{
    public class FacebookExternalLogin<TUser> : ExternalLoginBase<TUser>
        where TUser : IdentityUser<Guid>
    {
        private readonly FacebookClient facebookClient;

        public override string GrantType => Constants.GrantType;

        public FacebookExternalLogin(UserManager<TUser> userManager, FacebookClient facebookClient)
            : base(userManager)
        {
            this.facebookClient = facebookClient;
        }

        protected override async Task<string?> GetProviderIdAsync(string token)
        {
            try
            {
                var user = await facebookClient.GetUserInfoAsync(token);
                return user.Id;
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Cannot get Facebook user info");

                return null;
            }
        }
    }
}
