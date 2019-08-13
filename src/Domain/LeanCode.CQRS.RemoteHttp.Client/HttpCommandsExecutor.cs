using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LeanCode.CQRS.RemoteHttp.Client
{
    public class HttpCommandsExecutor : IRemoteCommandExecutor, IDisposable
    {
        private readonly JsonSerializerSettings serializerSettings;
        private readonly HttpClient client;

        public HttpCommandsExecutor(
            Uri baseAddress,
            JsonSerializerSettings settings = null)
        {
            client = new HttpClient
            {
                BaseAddress = baseAddress,
            };
            serializerSettings = settings;
        }

        public HttpCommandsExecutor(
            Uri baseAddress,
            HttpMessageHandler handler,
            JsonSerializerSettings settings = null)
        {
            client = new HttpClient(handler)
            {
                BaseAddress = baseAddress,
            };
            serializerSettings = settings;
        }

        public HttpCommandsExecutor(
            Uri baseAddress,
            HttpMessageHandler handler,
            bool disposeHandler,
            JsonSerializerSettings settings = null)
        {
            client = new HttpClient(handler, disposeHandler)
            {
                BaseAddress = baseAddress,
            };
            serializerSettings = settings;
        }

        public virtual async Task<CommandResult> RunAsync(IRemoteCommand command)
        {
            var stringified = JsonConvert.SerializeObject(command);
            using (var content = new StringContent(stringified, Encoding.UTF8, "application/json"))
            {
                using (var response = await client.PostAsync("command/" + command.GetType().FullName, content).ConfigureAwait(false))
                {
                    // Handle before HandleCommonCQRSErrors 'cause it will treat the 422 as "other error"
                    if ((int)response.StatusCode == 422)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<CommandResult>(
                            responseContent, serializerSettings);
                    }

                    response.HandleCommonCQRSErrors<CommandNotFoundException, InvalidCommandException>();

                    return CommandResult.Success();
                }
            }
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
