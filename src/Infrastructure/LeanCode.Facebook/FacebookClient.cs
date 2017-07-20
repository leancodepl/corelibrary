using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LeanCode.Facebook
{
    public class FacebookClient : IDisposable
    {
        private const string ApiBase = "https://graph.facebook.com/v2.9/";
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<FacebookClient>();

        private readonly string fieldsStr;
        private readonly HttpClient client;
        private readonly HMACSHA256 hmac;

        public FacebookClient(FacebookConfiguration config)
        {
            this.fieldsStr = GetFields(config.PhotoSize);
            this.hmac = new HMACSHA256(ParseKey(config.AppSecret));

            this.client = new HttpClient { BaseAddress = new Uri(ApiBase) };
        }

        public FacebookClient(
            FacebookConfiguration config,
            HttpMessageHandler innerHandler)
        {
            this.fieldsStr = GetFields(config.PhotoSize);
            this.hmac = new HMACSHA256(ParseKey(config.AppSecret));

            this.client = new HttpClient(innerHandler)
            {
                BaseAddress = new Uri(ApiBase)
            };
        }

        public async Task<FacebookUser> GetUserInfo(string accessToken)
        {
            var proof = GenerateProof(accessToken);
            var uri = $"me?fields={fieldsStr}&access_token={accessToken}&appsecret_proof={proof}";
            try
            {
                using (var response = await client.GetAsync(uri))
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        logger.Warning(
                            "Facebook Graph API returned {StatusCode} {reasonPhrase} for access token {FBAccessToken}",
                            response.StatusCode, response.ReasonPhrase, accessToken);
                        throw new FacebookException($"Cannot call Facebook Graph API, status: {response.StatusCode}.");
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    var result = JObject.Parse(content);

                    if (result["error"] != null)
                    {
                        var code = result["error"]["code"].Value<int>();
                        var msg = result["error"]["message"];
                        logger.Warning(
                            "User tried to use invalid access token {FBAccessToken}, Facebook returned {Code} error code with message {Message}",
                            accessToken, code, msg);
                        throw new FacebookException($"Cannot call Facebook Graph API. The call resulted in error {code} with message {msg}.");
                    }
                    else
                    {
                        var info = NewMethod(result);
                        logger.Information(
                            "Facebook user retrieved, user {FBUserId} has e-mail {FBEmail}",
                            info.Id, info.Email);
                        return info;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Cannot call Facebook Graph API");
                throw new FacebookException("Cannot call Facebook Graph API.", ex);
            }
        }

        private FacebookUser NewMethod(JObject result)
        {
            var id = result["id"].Value<string>();
            var email = result["email"]?.Value<string>();
            var firstName = result["first_name"]?.Value<string>();
            var lastName = result["last_name"]?.Value<string>();
            var photoUrl = result["picture"]["data"]["url"]?.Value<string>();
            return new FacebookUser(id, email, firstName, lastName, photoUrl);
        }

        public void Dispose()
        {
            client.Dispose();
        }

        private string GenerateProof(string accessToken)
        {
            var bytes = Encoding.ASCII.GetBytes(accessToken);
            var hash = hmac.ComputeHash(bytes);
            return ToHexString(hash);
        }

        private static byte[] ParseKey(string v)
        {
            return Encoding.ASCII.GetBytes(v);
        }

        private static string ToHexString(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", string.Empty).ToLower();
        }

        private static string GetFields(int photoSize)
        {
            return $"id,email,first_name,last_name,picture.width({photoSize}).height({photoSize})";
        }
    }
}
