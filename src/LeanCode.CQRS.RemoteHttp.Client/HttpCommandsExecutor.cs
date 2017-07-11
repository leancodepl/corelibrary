using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LeanCode.CQRS.RemoteHttp.Client
{
    public sealed class HttpCommandsExecutor : IRemoteCommandExecutor, IDisposable
    {
        private readonly HttpClient client;

        public HttpCommandsExecutor(Uri baseAddress)
        {
            client = new HttpClient
            {
                BaseAddress = baseAddress
            };
        }

        public HttpCommandsExecutor(Uri baseAddress, HttpMessageHandler handler)
        {
            client = new HttpClient(handler)
            {
                BaseAddress = baseAddress
            };
        }

        public HttpCommandsExecutor(Uri baseAddress, HttpMessageHandler handler, bool disposeHandler)
        {
            client = new HttpClient(handler, disposeHandler)
            {
                BaseAddress = baseAddress
            };
        }

        public async Task<CommandResult> RunAsync(IRemoteCommand command)
        {
            var stringified = JsonConvert.SerializeObject(command);
            using (var content = new StringContent(stringified, Encoding.UTF8, "application/json"))
            {
                using (var response = await client.PostAsync("command/" + command.GetType().FullName, content).ConfigureAwait(false))
                {
                    // Handle before HandleCommonCQRSErrors 'cause it will treat the 422 as "other error"
                    if ((int)response.StatusCode == 422)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        return JsonConvert.DeserializeObject<CommandResult>(responseContent);
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
