using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace LeanCode.ExternalIdentityProviders
{
    public abstract class ExternalLoginBase<TUser>
        where TUser : IdentityUser<Guid>
    {
        private readonly UserManager<TUser> userManager;

        protected Serilog.ILogger Logger { get; }
        public abstract string GrantType { get; }

        public ExternalLoginBase(UserManager<TUser> userManager)
        {
            this.userManager = userManager;

            Logger = Serilog.Log.ForContext(GetType());
        }

        public async Task<bool> IsConnectedAsync(Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            var logins = await userManager.GetLoginsAsync(user);

            return logins.Any(l => l.LoginProvider == GrantType);
        }

        public async Task<SignInResult> TrySignInAsync(string token)
        {
            var providerId = await GetProviderIdAsync(token);

            if (providerId is null)
            {
                return new SignInResult.TokenIsInvalid();
            }

            var user = await userManager.FindByLoginAsync(GrantType, providerId);

            if (user is null)
            {
                return new SignInResult.UserDoesNotExist();
            }
            else
            {
                return new SignInResult.Success(user.Id);
            }
        }

        public async Task ConnectAsync(Guid userId, string token)
        {
            var providerId = await GetProviderIdAsync(token);

            if (providerId is null)
            {
                throw new ExternalLoginException("The token is invalid.", TokenValidationError.Invalid);
            }
            else if (await userManager.FindByLoginAsync(GrantType, providerId) is not null)
            {
                throw new ExternalLoginException(
                    "Other account is already connected with this token.",
                    TokenValidationError.OtherConnected);
            }

            var user = await userManager.FindByIdAsync(userId.ToString());

            await userManager
                .AddLoginAsync(user, new(GrantType, providerId, GrantType))
                .EnsureIdentitySuccess();

            Logger.Information("User {UserId} connected their account with {GrantType}", userId, GrantType);
        }

        public async Task DisconnectAsync(Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            var logins = await userManager.GetLoginsAsync(user);

            if (logins.FirstOrDefault(l => l.LoginProvider == GrantType) is UserLoginInfo login)
            {
                await userManager
                    .RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey)
                    .EnsureIdentitySuccess();

                Logger.Information("User {UserId} disconnected their {GrantType} account", userId, GrantType);
            }
            else
            {
                Logger.Warning("User {UserId} does not have any {GrantType} account connected", userId, GrantType);
            }
        }

        public async Task<TokenValidationError?> ValidateConnectTokenAsync(string token)
        {
            var providerId = await GetProviderIdAsync(token);

            if (providerId is null)
            {
                return TokenValidationError.Invalid;
            }
            else if (await userManager.FindByLoginAsync(GrantType, providerId) is not null)
            {
                return TokenValidationError.OtherConnected;
            }
            else
            {
                return null;
            }
        }

        protected abstract Task<string?> GetProviderIdAsync(string token);
    }
}
