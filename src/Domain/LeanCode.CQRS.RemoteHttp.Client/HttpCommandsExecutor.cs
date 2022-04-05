using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using LeanCode.CQRS.Validation;

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

        public virtual async Task<CommandResult> RunAsync(IRemoteCommand command, CancellationToken cancellationToken = default)
        {
            using var content = JsonContent.Create(command, command.GetType(), options: serializerOptions);
            using var response = await client.PostAsync("command/" + command.GetType().FullName, content, cancellationToken);

            // Handle before HandleCommonCQRSErrors 'cause it will treat the 422 as "other error"
            if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
                await using var responseContent = await response.Content.ReadAsStreamAsync(cancellationToken);
                return await ParseResultAsync(responseContent, cancellationToken);
            }
            else
            {
                response.HandleCommonCQRSErrors<CommandNotFoundException, InvalidCommandException>();
                return CommandResult.Success;
            }
        }

        private async Task<CommandResult> ParseResultAsync(Stream responseContent, CancellationToken cancellationToken)
        {
            // `CommandResult` does not have default constructor and we want that to stay

            using var document = await ParseDocumentAsync(responseContent, cancellationToken);
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
                return new(errors.ToImmutableList());
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
                    propName.GetString() ?? "",
                    errMsg.GetString() ?? "",
                    errCode.GetInt32());
            }
            else
            {
                throw new MalformedResponseException();
            }
        }

        private async Task<JsonDocument> ParseDocumentAsync(Stream responseContent, CancellationToken cancellationToken)
        {
            try
            {
                var opts = new JsonDocumentOptions
                {
                    AllowTrailingCommas = serializerOptions.AllowTrailingCommas,
                    CommentHandling = serializerOptions.ReadCommentHandling,
                    MaxDepth = serializerOptions.MaxDepth,
                };
                return await JsonDocument.ParseAsync(responseContent, opts, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new MalformedResponseException(ex);
            }
        }
    }
}
