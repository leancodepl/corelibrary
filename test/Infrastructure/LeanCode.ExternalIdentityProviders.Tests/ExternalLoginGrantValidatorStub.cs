namespace LeanCode.ExternalIdentityProviders.Tests;

internal class ExternalLoginGrantValidatorStub : ExternalLoginGrantValidatorBase<User, ExternalLoginStub>
{
    public override string GrantType => "stub";

    public ExternalLoginGrantValidatorStub(ExternalLoginStub externalLogin)
        : base(externalLogin) { }
}
