using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using LeanCode.ExternalIdentityProviders.Facebook;
using Xunit;

namespace LeanCode.ExternalIdentityProviders.Tests.Facebook
{
    public class FacebookClientTests
    {
        private static readonly FacebookConfiguration Config = new(Environment.GetEnvironmentVariable("FACEBOOK_APP_SECRET") ?? string.Empty);
        private static readonly string AccessToken = Environment.GetEnvironmentVariable("FACEBOOK_TOKEN") ?? string.Empty;

        private readonly FacebookClient client;

        public FacebookClientTests()
        {
            client = new FacebookClient(
                Config,
                new HttpClient
                {
                    BaseAddress = new Uri(FacebookClient.ApiBase),
                });
        }

        [FacebookFact]
        public async Task Downloads_user_info_correctly()
        {
            var user = await client.GetUserInfoAsync(AccessToken);

            Assert.NotNull(user);
            Assert.NotEmpty(user.Id);
            Assert.NotEmpty(user.Email);
            Assert.NotEmpty(user.FirstName);
            Assert.NotEmpty(user.LastName);
            Assert.NotEmpty(user.Photo);
        }

        public class FacebookFactAttribute : FactAttribute
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
}
