using System.Threading.Tasks;
using Xunit;

namespace LeanCode.Facebook.Tests
{
    public class FacebookClientTests
    {
        private static readonly FacebookConfiguration Config = new FacebookConfiguration
        {
            PhotoSize = 200,
            AppSecret = string.Empty // Required for tests to work
        };
        private static readonly string AccessToken = string.Empty;

        private readonly FacebookClient client;

        public FacebookClientTests()
        {
            this.client = new FacebookClient(Config);
        }

        [Fact(Skip = "Facebook credentials required")]
        public async Task Downloads_user_info_correctly()
        {
            var user = await client.GetUserInfo(AccessToken);

            Assert.NotNull(user);
            Assert.NotEmpty(user.Id);
            Assert.NotEmpty(user.Email);
            Assert.NotEmpty(user.FirstName);
            Assert.NotEmpty(user.LastName);
            Assert.NotEmpty(user.Photo);
        }
    }
}
