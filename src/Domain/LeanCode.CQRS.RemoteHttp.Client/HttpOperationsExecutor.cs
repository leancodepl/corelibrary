using System.Net.Http.Json;
using System.Text.Json;
using LeanCode.Contracts;

namespace LeanCode.CQRS.RemoteHttp.Client
{
    public class HttpOperationsExecutor
    {
        private readonly HttpClient client;
        private readonly JsonSerializerOptions? serializerOptions;

        public HttpOperationsExecutor(HttpClient client)
            : this(client, null)
        { }

        public HttpOperationsExecutor(
            HttpClient client,
            JsonSerializerOptions? serializerOptions)
        {
            this.client = client;
            this.serializerOptions = serializerOptions ?? new JsonSerializerOptions();
        }

        public virtual async Task<TResult> GetAsync<TResult>(IOperation<TResult> operation, CancellationToken cancellationToken = default)
        {
            using var content = JsonContent.Create(operation, operation.GetType(), options: serializerOptions);
            using var response = await client.PostAsync("operation/" + operation.GetType().FullName, content, cancellationToken);

            response.HandleCommonCQRSErrors<OperationNotFoundException, InvalidOperationException>();

            using var responseContent = await response.Content.ReadAsStreamAsync(cancellationToken);
            try
            {
                return (await JsonSerializer.DeserializeAsync<TResult>(responseContent, serializerOptions, cancellationToken))!;
            }
            catch (Exception ex)
            {
                throw new MalformedResponseException(ex);
            }
        }
    }
}
