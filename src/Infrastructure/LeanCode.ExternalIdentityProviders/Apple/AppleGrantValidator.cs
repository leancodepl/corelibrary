using System;
using Microsoft.AspNetCore.Identity;

namespace LeanCode.ExternalIdentityProviders.Apple;

public class AppleGrantValidator<TUser> : ExternalLoginGrantValidatorBase<TUser, AppleExternalLogin<TUser>>
    where TUser : IdentityUser<Guid>
{
    public override string GrantType => Constants.GrantType;

    public AppleGrantValidator(AppleExternalLogin<TUser> externalLogin)
        : base(externalLogin)
    { }
}
