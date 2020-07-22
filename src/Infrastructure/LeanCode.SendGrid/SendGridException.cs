using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using SendGrid;

namespace LeanCode.SendGrid
{
    public class SendGridException : Exception
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public HttpStatusCode ResponseStatusCode { get; }
        public ImmutableArray<string> ErrorMessages { get; } = ImmutableArray<string>.Empty;

        private SendGridException(
            HttpStatusCode responseStatusCode,
            IEnumerable<SendGridError>? errors)
            : base($"SendGrid request failed with status code {responseStatusCode}.")
        {
            ResponseStatusCode = responseStatusCode;

            if (errors is object)
            {
                ErrorMessages = errors
                    .Select(e => e.Message!)
                    .Where(m => m is object)
                    .ToImmutableArray();
            }
        }

        public static async Task ThrowOnFailureAsync(Response response)
        {
            var statusCode = response.StatusCode;

            if (response.StatusCode >= HttpStatusCode.BadRequest)
            {
                await using var stream = await response.Body.ReadAsStreamAsync();

                try
                {
                    var body = await JsonSerializer.DeserializeAsync<SendGridResponse>(stream, SerializerOptions);

                    throw new SendGridException(statusCode, body?.Errors);
                }
                catch (JsonException)
                {
                    throw new SendGridException(statusCode, null);
                }
            }
        }

        private class SendGridResponse
        {
            public SendGridError[]? Errors { get; set; }
        }

        private class SendGridError
        {
            public string? Message { get; set; }
        }
    }
}
