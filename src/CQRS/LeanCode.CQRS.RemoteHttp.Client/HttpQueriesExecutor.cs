using System.Net.Http.Json;
using System.Text.Json;
using LeanCode.Contracts;

namespace LeanCode.CQRS.RemoteHttp.Client;

public class HttpQueriesExecutor
{
    private readonly HttpClient client;
    private readonly JsonSerializerOptions? serializerOptions;

    public HttpQueriesExecutor(HttpClient client)
        : this(client, null) { }

    public HttpQueriesExecutor(HttpClient client, JsonSerializerOptions? serializerOptions)
    {
        this.client = client;
        this.serializerOptions = serializerOptions ?? new JsonSerializerOptions();
    }

    public virtual async Task<TResult> GetAsync<TResult>(
        IQuery<TResult> query,
        CancellationToken cancellationToken = default
    )
    {
        using var content = JsonContent.Create(query, query.GetType(), options: serializerOptions);
        using var response = await client.PostAsync(
            new Uri("query/" + query.GetType().FullName, UriKind.Relative),
            content,
            cancellationToken
        );

        response.HandleCommonCQRSErrors<QueryNotFoundException, InvalidQueryException>();

        using var responseContent = await response.Content.ReadAsStreamAsync(cancellationToken);
        try
        {
            return (
                await JsonSerializer.DeserializeAsync<TResult>(responseContent, serializerOptions, cancellationToken)
            )!;
        }
        catch (Exception ex)
        {
            throw new MalformedResponseException(ex);
        }
    }
}
