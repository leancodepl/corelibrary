using System;
using Microsoft.AspNetCore.Identity;

namespace LeanCode.ExternalIdentityProviders.Facebook;

public class FacebookGrantValidator<TUser> : ExternalLoginGrantValidatorBase<TUser, FacebookExternalLogin<TUser>>
    where TUser : IdentityUser<Guid>
{
    public override string GrantType => Constants.GrantType;

    public FacebookGrantValidator(FacebookExternalLogin<TUser> externalLogin)
        : base(externalLogin)
    { }
}
