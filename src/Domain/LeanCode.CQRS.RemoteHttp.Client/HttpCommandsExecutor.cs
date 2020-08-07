using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LeanCode.CQRS.Validation;
using LeanCode.Serialization;

namespace LeanCode.CQRS.RemoteHttp.Client
{
    public class HttpCommandsExecutor
    {
        private readonly HttpClient client;
        private readonly JsonSerializerOptions serializerOptions;

        public HttpCommandsExecutor(HttpClient client)
            : this(client, null)
        { }

        public HttpCommandsExecutor(
            HttpClient client,
            JsonSerializerOptions? serializerOptions)
        {
            this.client = client;
            this.serializerOptions = serializerOptions ?? new JsonSerializerOptions();
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
                return await ParseResultAsync(responseContent);
            }
            else
            {
                response.HandleCommonCQRSErrors<CommandNotFoundException, InvalidCommandException>();
                return CommandResult.Success;
            }
        }

        private async Task<CommandResult> ParseResultAsync(Stream responseContent)
        {
            // `CommandResult` does not have default constructor and we want that to stay

            using var document = await ParseDocumentAsync(responseContent);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new MalformedResponseException();
            }
            else if (document.RootElement.TryGetProperty(nameof(CommandResult.WasSuccessful), out var wasSuccessful) &&
                wasSuccessful.GetBoolean())
            {
                return CommandResult.Success;
            }
            else if (document.RootElement.TryGetProperty(nameof(CommandResult.ValidationErrors), out var validationErrors) &&
                validationErrors.ValueKind == JsonValueKind.Array)
            {
                var errors = validationErrors.EnumerateArray().Select(ParseError).ToList();
                return new CommandResult(errors);
            }
            else
            {
                throw new MalformedResponseException();
            }
        }

        private ValidationError ParseError(JsonElement el)
        {
            if (el.TryGetProperty(nameof(ValidationError.ErrorCode), out var errCode) &&
                el.TryGetProperty(nameof(ValidationError.ErrorMessage), out var errMsg) &&
                el.TryGetProperty(nameof(ValidationError.PropertyName), out var propName) &&
                errCode.ValueKind == JsonValueKind.Number &&
                errMsg.ValueKind == JsonValueKind.String &&
                propName.ValueKind == JsonValueKind.String)
            {
                return new ValidationError(
                    propName.GetString() ?? string.Empty,
                    errMsg.GetString() ?? string.Empty,
                    errCode.GetInt32());
            }
            else
            {
                throw new MalformedResponseException();
            }
        }

        private async Task<JsonDocument> ParseDocumentAsync(Stream responseContent)
        {
            try
            {
                var opts = new JsonDocumentOptions
                {
                    AllowTrailingCommas = serializerOptions.AllowTrailingCommas,
                    CommentHandling = serializerOptions.ReadCommentHandling,
                    MaxDepth = serializerOptions.MaxDepth,
                };
                return await JsonDocument.ParseAsync(responseContent, opts);
            }
            catch (Exception ex)
            {
                throw new MalformedResponseException(ex);
            }
        }
    }
}
