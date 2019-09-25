using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LeanCode.Serialization;

namespace LeanCode.CQRS.RemoteHttp.Client
{
    public class HttpCommandsExecutor : IRemoteCommandExecutor, IDisposable
    {
        private readonly HttpClient client;
        private readonly JsonSerializerOptions? serializerOptions;

        public HttpCommandsExecutor(Uri baseAddress, JsonSerializerOptions? options = null)
        {
            client = new HttpClient()
            {
                BaseAddress = baseAddress,
            };

            serializerOptions = options;
        }

        public HttpCommandsExecutor(
            Uri baseAddress,
            HttpMessageHandler handler,
            JsonSerializerOptions? options = null)
        {
            client = new HttpClient(handler)
            {
                BaseAddress = baseAddress,
            };

            serializerOptions = options;
        }

        public HttpCommandsExecutor(
            Uri baseAddress,
            HttpMessageHandler handler,
            bool disposeHandler,
            JsonSerializerOptions? options = null)
        {
            client = new HttpClient(handler, disposeHandler)
            {
                BaseAddress = baseAddress,
            };

            serializerOptions = options;
        }

        public virtual async Task<CommandResult> RunAsync(IRemoteCommand command)
        {
            using var content = JsonContent.Create(command, command.GetType());
            using var response = await client
                .PostAsync("command/" + command.GetType().FullName, content);

            // Handle before HandleCommonCQRSErrors 'cause it will treat the 422 as "other error"
            if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
                await using var responseContent = await response.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<CommandResult>(responseContent, serializerOptions);
            }

            response.HandleCommonCQRSErrors<CommandNotFoundException, InvalidCommandException>();

            return CommandResult.Success;
        }

        public void Dispose() => client.Dispose();
    }
}
