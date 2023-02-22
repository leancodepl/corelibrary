using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using LeanCode.ExternalIdentityProviders.Facebook;
using Xunit;

namespace LeanCode.ExternalIdentityProviders.Tests.Facebook;

public sealed class FacebookClientTests : IDisposable
{
    private static readonly FacebookConfiguration Config = new(Environment.GetEnvironmentVariable("FACEBOOK_APP_SECRET") ?? "");
    private static readonly string AccessToken = Environment.GetEnvironmentVariable("FACEBOOK_TOKEN") ?? "";

    private readonly FacebookClient client;

    [SuppressMessage("?", "CA2000", Justification = "References don't go out of scope.")]
    public FacebookClientTests()
    {
        client = new FacebookClient(
            Config,
            new HttpClient
            {
                BaseAddress = new Uri(FacebookClient.ApiBase),
            });
    }

    public void Dispose() => ((IDisposable)client).Dispose();

    [FacebookFact]
    public async Task Downloads_user_info_correctly()
    {
        var user = await client.GetUserInfoAsync(AccessToken);

        Assert.NotNull(user);

        Assert.NotEmpty(user.Id);

        Assert.NotNull(user.Email);
        Assert.NotEmpty(user.Email);

        Assert.NotNull(user.FirstName);
        Assert.NotEmpty(user.FirstName);

        Assert.NotNull(user.LastName);
        Assert.NotEmpty(user.LastName);

        Assert.NotEmpty(user.Photo);
    }

    internal sealed class FacebookFactAttribute : FactAttribute
    {
        public FacebookFactAttribute()
        {
            if (string.IsNullOrEmpty(Config.AppSecret))
            {
                Skip = "API key not set";
            }
            else if (string.IsNullOrEmpty(AccessToken))
            {
                Skip = "No token provided";
            }
        }
    }
}
