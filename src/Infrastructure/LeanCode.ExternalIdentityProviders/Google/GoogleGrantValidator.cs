using System;
using Microsoft.AspNetCore.Identity;

namespace LeanCode.ExternalIdentityProviders.Google
{
    public class GoogleGrantValidator<TUser> : ExternalLoginGrantValidatorBase<TUser, GoogleExternalLogin<TUser>>
        where TUser : IdentityUser<Guid>
    {
        public override string GrantType => Constants.GrantType;

        public GoogleGrantValidator(GoogleExternalLogin<TUser> externalLogin)
            : base(externalLogin)
        { }
    }
}
