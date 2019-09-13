using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static LeanCode.PushNotifications.FCMResult;

namespace LeanCode.PushNotifications
{
    public class FCMClient : IDisposable
    {
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
        };

        private readonly HttpClient client;

        /// <param name="client">Preconfigured HttpClient.</param>
        public FCMClient(HttpClient client)
        {
            this.client = client;
        }

        public virtual async Task<FCMResult> Send(FCMNotification notification)
        {
            var stringified = JsonConvert.SerializeObject(notification, settings);

            using var content = new StringContent(stringified, Encoding.UTF8, "application/json");

            using var response = await client
                .PostAsync(string.Empty, content)
                .ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new HttpError(response.StatusCode);
            }

            var resultStr = await response.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

            var result = JObject.Parse(resultStr);

            if (result["canonical_ids"].Value<int>() == 1)
            {
                return new TokenUpdated(result["results"][0]["registration_id"].Value<string>());
            }

            if (result["failure"].Value<int>() == 1)
            {
                var error = result["results"][0]["error"].Value<string>();

                if (error == "NotRegistered" || error == "InvalidRegistration")
                {
                    return new InvalidToken();
                }
                else
                {
                    return new OtherError(error);
                }
            }

            return new Success();
        }

        public void Dispose() => client.Dispose();
    }
}
