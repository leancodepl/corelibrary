namespace LeanCode.ExternalIdentityProviders.Tests;

internal sealed class ExternalLoginGrantValidatorStub : ExternalLoginGrantValidatorBase<User, ExternalLoginStub>
{
    public override string GrantType => "stub";

    public ExternalLoginGrantValidatorStub(ExternalLoginStub externalLogin)
        : base(externalLogin) { }
}
