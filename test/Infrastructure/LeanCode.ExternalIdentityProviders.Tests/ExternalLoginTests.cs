using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace LeanCode.ExternalIdentityProviders.Tests;

public class ExternalLoginTests
{
    private readonly UserManager<User> users = UserManager.PrepareInMemory();
    private readonly ExternalLoginStub externalLogin;

    public ExternalLoginTests()
    {
        externalLogin = new ExternalLoginStub(users);
    }

    [Fact]
    public async Task Non_existing_user_is_not_connected()
    {
        var res = await externalLogin.IsConnectedAsync(Guid.NewGuid());

        Assert.False(res);
    }

    [Fact]
    public async Task New_useris_not_connected()
    {
        var uid = await AddUserAsync();
        var res = await externalLogin.IsConnectedAsync(uid);

        Assert.False(res);
    }

    [Fact]
    public async Task Cannot_sign_in_with_unknown_userid()
    {
        var token = AddToken();

        var res = await externalLogin.TrySignInAsync(token);

        Assert.IsType<SignInResult.UserDoesNotExist>(res);
    }

    [Fact]
    public async Task Cannot_sign_in_with_invalid_token()
    {
        var res = await externalLogin.TrySignInAsync("token");

        Assert.IsType<SignInResult.TokenIsInvalid>(res);
    }

    [Fact]
    public async Task Cannot_connect_with_invalid_token()
    {
        var uid = await AddUserAsync();

        await Assert.ThrowsAsync<ExternalLoginException>(() => externalLogin.ConnectAsync(uid, "token2"));
    }

    [Fact]
    public async Task Cannot_connect_with_non_existing_user()
    {
        var token = AddToken();

        await Assert.ThrowsAsync<ExternalLoginException>(() => externalLogin.ConnectAsync(Guid.NewGuid(), token));
    }

    [Fact]
    public async Task Cannot_conenct_the_same_user_twice()
    {
        var token = AddToken();
        var uid = await AddUserAsync();

        await externalLogin.ConnectAsync(uid, token);
        await Assert.ThrowsAsync<ExternalLoginException>(() => externalLogin.ConnectAsync(uid, token));
    }

    [Fact]
    public async Task Cannot_conenct_different_account_using_the_same_token()
    {
        var token = AddToken();
        var uid1 = await AddUserAsync();
        var uid2 = await AddUserAsync();

        await externalLogin.ConnectAsync(uid1, token);
        await Assert.ThrowsAsync<ExternalLoginException>(() => externalLogin.ConnectAsync(uid2, token));
    }

    [Fact]
    public async Task After_connecting_the_user_is_connected()
    {
        var token = AddToken();
        var uid = await AddUserAsync();

        await externalLogin.ConnectAsync(uid, token);

        Assert.True(await externalLogin.IsConnectedAsync(uid));
    }

    [Fact]
    public async Task After_connecting_the_user_can_sign_in()
    {
        var token = AddToken();
        var uid = await AddUserAsync();

        await externalLogin.ConnectAsync(uid, token);

        var res = await externalLogin.TrySignInAsync(token);
        var success = Assert.IsType<SignInResult.Success>(res);
        Assert.Equal(uid, success.UserId);
    }

    [Fact]
    public async Task After_connecting_and_disconnecting_user_is_no_longer_connected()
    {
        var token = AddToken();
        var uid = await AddUserAsync();

        await externalLogin.ConnectAsync(uid, token);
        await externalLogin.DisconnectAsync(uid);

        Assert.False(await externalLogin.IsConnectedAsync(uid));
    }

    [Fact]
    public async Task After_connecting_and_disconnecting_token_can_be_reused()
    {
        var token = AddToken();
        var uid1 = await AddUserAsync();
        var uid2 = await AddUserAsync();

        await externalLogin.ConnectAsync(uid1, token);
        await externalLogin.DisconnectAsync(uid1);

        await externalLogin.ConnectAsync(uid2, token);

        Assert.False(await externalLogin.IsConnectedAsync(uid1));
        Assert.True(await externalLogin.IsConnectedAsync(uid2));
    }

    [Fact]
    public async Task Different_user_can_connect_with_different_tokens()
    {
        var token1 = AddToken();
        var token2 = AddToken();
        var uid1 = await AddUserAsync();
        var uid2 = await AddUserAsync();

        await externalLogin.ConnectAsync(uid1, token1);
        await externalLogin.ConnectAsync(uid2, token2);

        Assert.True(await externalLogin.IsConnectedAsync(uid1));
        Assert.True(await externalLogin.IsConnectedAsync(uid2));
    }

    [Fact]
    public async Task Disconnecting_not_connected_user_is_noop()
    {
        var uid = await AddUserAsync();

        await externalLogin.DisconnectAsync(uid);

        Assert.False(await externalLogin.IsConnectedAsync(uid));
    }

    [Fact]
    public async Task Disconnecting_not_existing_user_is_noop()
    {
        await externalLogin.DisconnectAsync(Guid.NewGuid());
    }

    [Fact]
    public async Task Invalid_token_is_validated_as_invalid()
    {
        var res = await externalLogin.ValidateConnectTokenAsync("token");

        Assert.Equal(TokenValidationError.Invalid, res);
    }

    [Fact]
    public async Task Correct_not_connected_token_is_valid()
    {
        var token = AddToken();

        var res = await externalLogin.ValidateConnectTokenAsync(token);

        Assert.Null(res);
    }

    [Fact]
    public async Task Valid_but_connected_token_is_invalid()
    {
        var token = AddToken();
        var uid = await AddUserAsync();
        await externalLogin.ConnectAsync(uid, token);

        var res = await externalLogin.ValidateConnectTokenAsync(token);

        Assert.Equal(TokenValidationError.OtherConnected, res);
    }

    private Task<Guid> AddUserAsync() => users.AddUserAsync();

    private string AddToken() => externalLogin.AddToken();
}
