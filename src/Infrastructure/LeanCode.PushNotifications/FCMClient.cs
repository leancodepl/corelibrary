using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LeanCode.PushNotifications
{
    public class FCMClient : IDisposable
    {
        private readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions()
        {
            IgnoreNullValues = true,
        };

        private readonly HttpClient client;

        /// <param name="client">Preconfigured HttpClient.</param>
        public FCMClient(HttpClient client)
        {
            this.client = client;
        }

        public virtual async Task<FCMResult> SendAsync(FCMNotification notification)
        {
            using var content = PrepareContent(notification);
            using var response = await client.PostAsync(string.Empty, content);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new FCMResult.HttpError(response.StatusCode);
            }

            await using var resultStream = await response.Content.ReadAsStreamAsync();
            using var result = await JsonDocument.ParseAsync(resultStream);
            var root = result.RootElement;

            if (root.GetProperty("canonical_ids").GetInt32() == 1)
            {
                var id = root.GetProperty("results")[0].GetProperty("registration_id").GetString();
                return new FCMResult.TokenUpdated(id);
            }
            else if (result.RootElement.GetProperty("failure").GetInt32() == 1)
            {
                var error = root.GetProperty("results")[0].GetProperty("error").GetString();

                if (error == "NotRegistered" || error == "InvalidRegistration")
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

        public void Dispose() => client.Dispose();

        private ByteArrayContent PrepareContent(FCMNotification notification)
        {
            var payload = JsonSerializer.SerializeToUtf8Bytes(notification, serializerOptions);
            var content = new ByteArrayContent(payload);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
            {
                CharSet = Encoding.UTF8.WebName,
            };
            return content;
        }
    }
}
