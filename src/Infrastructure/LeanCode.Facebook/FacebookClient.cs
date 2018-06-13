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
        private const string ApiBase = "https://graph.facebook.com/v3.0/";
        private const string FieldsStr = "id,email,first_name,last_name";

        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<FacebookClient>();

        private readonly HttpClient client;
        private readonly HMACSHA256 hmac;

        private readonly int photoSize;

        public FacebookClient(FacebookConfiguration config)
        {
            this.photoSize = config.PhotoSize;
            this.hmac = config.AppSecret == null ? null : new HMACSHA256(ParseKey(config.AppSecret));

            this.client = new HttpClient
            {
                BaseAddress = new Uri(ApiBase)
            };
        }

        public FacebookClient(
            FacebookConfiguration config,
            HttpMessageHandler innerHandler)
        {
            this.photoSize = config.PhotoSize;
            this.hmac = config.AppSecret == null ? null : new HMACSHA256(ParseKey(config.AppSecret));

            this.client = new HttpClient(innerHandler)
            {
                BaseAddress = new Uri(ApiBase)
            };
        }

        public virtual async Task<JObject> CallAsync(string endpoint, string accessToken, bool handleError = true)
        {
            var proof = GenerateProof(accessToken);
            var uri = AppendProof(endpoint, accessToken, proof);
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

                    if (handleError && result["error"] != null)
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
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Cannot call Facebook Graph API");
                throw new FacebookException("Cannot call Facebook Graph API.", ex);
            }
        }

        public virtual async Task<FacebookUser> GetUserInfo(string accessToken)
        {
            var result = await CallAsync("me?fields=" + FieldsStr, accessToken);
            var info = ConvertResponse(result);
            logger.Information(
                "Facebook user retrieved, user {FBUserId} has e-mail {FBEmail}",
                info.Id, info.Email);
            return info;
        }

        private FacebookUser ConvertResponse(JObject result)
        {
            var id = result["id"].Value<string>();
            var email = result["email"]?.Value<string>();
            var firstName = result["first_name"]?.Value<string>();
            var lastName = result["last_name"]?.Value<string>();
            var photoUrl = $"{ApiBase}{id}/picture?width={photoSize}&height={photoSize}";
            return new FacebookUser(id, email, firstName, lastName, photoUrl);
        }

        public void Dispose()
        {
            client.Dispose();
        }

        private string GenerateProof(string accessToken)
        {
            if (hmac != null)
            {
                logger.Debug("Signing Facebook request enabled - signing request");
                var bytes = Encoding.ASCII.GetBytes(accessToken);
                var hash = hmac.ComputeHash(bytes);
                return "&appsecret_proof=" + ToHexString(hash);
            }
            else
            {
                logger.Debug("Signing Facebook requests is disabled, skipping generating proof");
                return string.Empty;
            }
        }

        private static byte[] ParseKey(string v)
        {
            return Encoding.ASCII.GetBytes(v);
        }

        private static string ToHexString(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", string.Empty).ToLower();
        }

        private static string AppendProof(string uri, string accessToken, string proof)
        {
            if (uri.Contains("?"))
            {
                return uri + "&access_token=" + accessToken + proof;
            }
            else
            {
                return uri + "?access_token=" + accessToken + proof;
            }
        }
    }
}
