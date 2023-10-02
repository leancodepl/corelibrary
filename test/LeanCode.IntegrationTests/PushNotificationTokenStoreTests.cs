using System.Diagnostics.CodeAnalysis;
using LeanCode.Firebase.FCM;
using LeanCode.IntegrationTests.App;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.IntegrationTests;

[SuppressMessage("?", "CA1001", Justification = "Disposal is handled by IAsyncLifetime interface.")]
public class PushNotificationTokenStoreTests : IAsyncLifetime
{
    private readonly TestApp app;
    private PushNotificationTokenStore<TestDbContext, Guid> store = default!;

    public PushNotificationTokenStoreTests()
    {
        app = new TestApp();
    }

    [Fact]
    public async Task Gets_freshly_saved_token()
    {
        const string Token = "token";
        var uid = Guid.NewGuid();

        await store.AddUserTokenAsync(uid, Token);
        var result = await store.GetTokensAsync(uid);

        Assert.Equal(new() { Token }, result);
    }

    [Fact]
    public async Task Gets_multiple_freshly_saved_token()
    {
        const string Token1 = "token1";
        const string Token2 = "token2";
        var uid = Guid.NewGuid();

        await store.AddUserTokenAsync(uid, Token1);
        await store.AddUserTokenAsync(uid, Token2);
        var result = await store.GetTokensAsync(uid);

        Assert.Equal(new() { Token1, Token2 }, result);
    }

    [Fact]
    public async Task Gets_tokens_for_multiple_users()
    {
        const string Token1 = "token1";
        const string Token2 = "token2";
        var uid1 = Guid.NewGuid();
        var uid2 = Guid.NewGuid();

        await store.AddUserTokenAsync(uid1, Token1);
        await store.AddUserTokenAsync(uid2, Token2);

        var result = await store.GetTokensAsync(new HashSet<Guid> { uid1, uid2 });

        Assert.Equal(2, result.Count);
        var first = Assert.Single(result, e => e.Key == uid1);
        var firstToken = Assert.Single(first.Value);
        Assert.Equal(Token1, firstToken);

        var second = Assert.Single(result, e => e.Key == uid2);
        var secondToken = Assert.Single(second.Value);
        Assert.Equal(Token2, secondToken);
    }

    [Fact]
    public async Task Removes_single_token()
    {
        const string Token1 = "token1";
        const string Token2 = "token2";
        var uid = Guid.NewGuid();

        await store.AddUserTokenAsync(uid, Token1);
        await store.AddUserTokenAsync(uid, Token2);
        await store.RemoveTokenAsync(Token1);
        var result = await store.GetTokensAsync(uid);

        Assert.Equal(new() { Token2 }, result);
    }

    [Fact]
    public async Task Removes_user_tokens()
    {
        const string Token1 = "token1";
        const string Token2 = "token2";
        var uid1 = Guid.NewGuid();
        var uid2 = Guid.NewGuid();

        await store.AddUserTokenAsync(uid1, Token1);
        await store.AddUserTokenAsync(uid2, Token2);

        await store.RemoveUserTokenAsync(uid1, Token2); // Noop
        await store.RemoveUserTokenAsync(uid1, Token1);

        var result1 = await store.GetTokensAsync(uid1);
        var result2 = await store.GetTokensAsync(uid2);

        Assert.Empty(result1);
        Assert.Equal(new() { Token2 }, result2);
    }

    [Fact]
    public async Task Removes_multiple_tokens()
    {
        const string Token1 = "token1";
        const string Token2 = "token2";
        var uid1 = Guid.NewGuid();
        var uid2 = Guid.NewGuid();

        await store.AddUserTokenAsync(uid1, Token1);
        await store.AddUserTokenAsync(uid2, Token2);

        await store.RemoveTokensAsync(new[] { Token1, Token2 });

        var result1 = await store.GetTokensAsync(uid1);
        var result2 = await store.GetTokensAsync(uid2);

        Assert.Empty(result1);
        Assert.Empty(result2);
    }

    public async Task InitializeAsync()
    {
        await app.InitializeAsync();

        store = new(app.Services.GetRequiredService<TestDbContext>());
    }

    public async Task DisposeAsync()
    {
        await app.DisposeAsync();
    }
}
