using System;
using System.Net;
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
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new CommandNotFoundException();
                    }
                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        throw new InvalidCommandException();
                    }
                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        throw new InternalServerErrorException();
                    }
                    if ((int)response.StatusCode == 422)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        return JsonConvert.DeserializeObject<CommandResult>(responseContent);
                    }
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
