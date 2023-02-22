using System;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;

namespace LeanCode.ExternalIdentityProviders;

public abstract class ExternalLoginGrantValidatorBase<TUser, TExternalLogin> : IExtensionGrantValidator
    where TUser : IdentityUser<Guid>
    where TExternalLogin : ExternalLoginBase<TUser>
{
    public const string AssertionField = "assertion";

    private readonly TExternalLogin externalLogin;

    public abstract string GrantType { get; }

    protected ExternalLoginGrantValidatorBase(TExternalLogin externalLogin)
    {
        this.externalLogin = externalLogin;
    }

    public async Task ValidateAsync(ExtensionGrantValidationContext context)
    {
        var token = context.Request.Raw[AssertionField];

        if (string.IsNullOrEmpty(token))
        {
            context.Result = new GrantValidationResult(
                TokenRequestErrors.InvalidGrant,
                GrantValidationErrors.NoAssertion);

            return;
        }

        context.Result = await externalLogin.TrySignInAsync(token) switch
        {
            SignInResult.Success s => new(s.UserId.ToString(), GrantType),

            SignInResult.UserDoesNotExist => new(
                TokenRequestErrors.InvalidGrant,
                GrantValidationErrors.NoUser),

            SignInResult.TokenIsInvalid => new(
                TokenRequestErrors.InvalidGrant,
                GrantValidationErrors.InvalidAssertion),

            var o => throw new ExternalLoginException($"Unexpected SignInResult of type {o.GetType().Name}."),
        };
    }
}
