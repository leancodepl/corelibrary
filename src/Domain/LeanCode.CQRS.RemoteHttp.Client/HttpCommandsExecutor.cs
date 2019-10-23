using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LeanCode.Serialization;

namespace LeanCode.CQRS.RemoteHttp.Client
{
    public class HttpCommandsExecutor
    {
        private readonly HttpClient client;
        private readonly JsonSerializerOptions? serializerOptions;

        public HttpCommandsExecutor(HttpClient client)
            : this(client, null)
        { }

        public HttpCommandsExecutor(
            HttpClient client,
            JsonSerializerOptions? serializerOptions)
        {
            this.client = client;
            this.serializerOptions = serializerOptions ?? new JsonSerializerOptions();

            this.serializerOptions.Converters.Add(new CommandResultConverter());
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
                var result = await JsonSerializer.DeserializeAsync<CommandResult?>(responseContent, serializerOptions);
                if (result is null)
                {
                    throw new MalformedResponseException();
                }
                else
                {
                    return result;
                }
            }
            else
            {
                response.HandleCommonCQRSErrors<CommandNotFoundException, InvalidCommandException>();
                return CommandResult.Success;
            }
        }
    }
}
