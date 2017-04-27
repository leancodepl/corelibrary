using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LeanCode.PushNotifications
{
    class FCMClient : IFCMClient
    {
        private const string Endpoint = "https://fcm.googleapis.com/fcm/send";
        private readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        private readonly HttpClient client;

        public FCMClient(FCMConfiguration configuration)
        {
            client = new HttpClient
            {
                BaseAddress = new Uri(Endpoint)
            };

            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "key=" + configuration.ApiKey);
        }

        public FCMClient(IOptions<FCMConfiguration> configuration)
            : this(configuration.Value)
        { }

        public async Task<FCMResult> Send(FCMNotification notification)
        {
            var stringified = JsonConvert.SerializeObject(notification, Settings);
            using (var content = new StringContent(stringified, Encoding.UTF8, "application/json"))
            {
                using (var response = await client.PostAsync("", content).ConfigureAwait(false))
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return new FCMResult.HttpError(response.StatusCode);
                    }

                    var resultStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var result = JObject.Parse(resultStr);

                    if (result["canonical_ids"].Value<int>() == 1)
                    {
                        var newToken = result["results"][0]["registration_id"].Value<string>();
                        return new FCMResult.TokenUpdated(newToken);
                    }
                    else if (result["failure"].Value<int>() == 1)
                    {
                        var error = result["results"][0]["error"].Value<string>();
                        if (error == "NotRegistered")
                        {
                            return new FCMResult.InvalidToken();
                        }
                        else
                        {
                            return new FCMResult.OtherError(error);
                        }
                    }
                    else
                    {
                        return new FCMResult.Success();
                    }
                }
            }
        }
    }
}
