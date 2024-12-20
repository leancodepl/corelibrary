using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using LeanCode.ExternalIdentityProviders.Facebook;
using LeanCode.Test.Helpers;
using Xunit;

namespace LeanCode.ExternalIdentityProviders.Tests.Facebook;

public sealed class FacebookClientTests : IDisposable
{
    private static readonly FacebookConfiguration Config =
        new(Environment.GetEnvironmentVariable("FACEBOOK_APP_SECRET") ?? "");
    private static readonly string AccessToken = Environment.GetEnvironmentVariable("FACEBOOK_TOKEN") ?? "";

    private readonly HttpClient httpClient;
    private readonly FacebookClient client;

    public FacebookClientTests()
    {
        httpClient = new HttpClient { BaseAddress = new Uri(FacebookClient.ApiBase), };
        client = new FacebookClient(Config, httpClient);
    }

    public void Dispose() => httpClient.Dispose();

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
}

internal sealed class FacebookFactAttribute : ExternalServiceFactAttribute
{
    protected override IReadOnlyCollection<string> RequiredEnvVariables { get; } =
        ["FACEBOOK_APP_SECRET", "FACEBOOK_TOKEN"];
}
