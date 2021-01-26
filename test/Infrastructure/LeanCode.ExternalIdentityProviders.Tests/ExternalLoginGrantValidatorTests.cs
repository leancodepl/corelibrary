using System;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace LeanCode.ExternalIdentityProviders.Tests
{
    public class ExternalLoginGrantValidatorTests
    {
        private readonly UserManager<User> users = UserManager.PrepareInMemory();
        private readonly ExternalLoginStub externalLogin;
        private readonly ExternalLoginGrantValidatorStub grantValidator;

        public ExternalLoginGrantValidatorTests()
        {
            externalLogin = new ExternalLoginStub(users);
            grantValidator = new ExternalLoginGrantValidatorStub(externalLogin);
        }

        [Fact]
        public async Task Passing_null_token_results_in_no_assertion_error()
        {
            var res = await ValidateAsync(null);

            res.AssertInvalid("no_assertion");
        }

        [Fact]
        public async Task Passing_empty_token_results_in_no_assertion_error()
        {
            var res = await ValidateAsync(string.Empty);

            res.AssertInvalid("no_assertion");
        }

        [Fact]
        public async Task Passing_invalid_token_results_in_invalid_assertion_error()
        {
            var res = await ValidateAsync("test");

            res.AssertInvalid("invalid_assertion");
        }

        [Fact]
        public async Task Passing_not_connected_token_results_in_no_user_error()
        {
            var token = AddToken();
            var res = await ValidateAsync(token);

            res.AssertInvalid("no_user");
        }

        [Fact]
        public async Task Passing_connected_token_succeeds()
        {
            var (token, uid) = await AddConnectedUserAsync();
            var res = await ValidateAsync(token);

            res.AssertValid(uid);
        }

        private async Task<GrantValidationResult> ValidateAsync(string? token)
        {
            var ctx = new ExtensionGrantValidationContext
            {
                Request = new ValidatedTokenRequest
                {
                    Raw = new System.Collections.Specialized.NameValueCollection
                    {
                        ["assertion"] = token,
                    },
                },
            };
            await grantValidator.ValidateAsync(ctx);

            Assert.NotNull(ctx.Result);
            return ctx.Result;
        }

        private async Task<(string, Guid)> AddConnectedUserAsync()
        {
            var token = AddToken();
            var uid = await users.AddUserAsync();
            await externalLogin.ConnectAsync(uid, token);
            return (token, uid);
        }

        private string AddToken() => externalLogin.AddToken();
    }

    internal static class GrantValidationResultExtensions
    {
        public static void AssertValid(this GrantValidationResult result, Guid uid)
        {
            Assert.False(result.IsError);
            Assert.NotNull(result.Subject);
            Assert.Equal(uid.ToString(), result.Subject.FindFirst("sub")?.Value);
        }

        public static void AssertInvalid(this GrantValidationResult result, string error)
        {
            Assert.True(result.IsError);
            Assert.Equal("invalid_grant", result.Error);
            Assert.Equal(error, result.ErrorDescription);
        }
    }
}
