## Adding new query
In order to create query you have to follow those steps.

### 1. Create Query
Query creation requires creating class that implements `IQuery<TResult>` interface. `TResult` parameter specifies the type that query returns when executed. Query should define properties with will be later use in QueryHandler. Defining a Query, you can specify Attributes such as `[AllowUnauthorized]` or `[AuthorizeWhenHasAnyOf("Some_permission")]` to pass additional metadata.
There are example Query and QueryResult classes below:
```
using LeanCode.Contracts;
using LeanCode.Contracts.Security;

namespace LncdApp.DomainName.Contracts.Example;

[AllowUnauthorized]
public class ExampleQuery : IQuery<QueryResultDTO>
{
    public string Name { get; set; }
}

public class QueryResultDTO
{
    public string Greeting { get; set; }
}
```
Important things is that TResult should be DTO (but it can also be an enum) because most of the time it will be serialized.

Best practices:
- Query should be named to fill in the gap in "get ..." sentence. For example if query is suposed to get all users - the query should be named AllUsers.

### 2. Create QueryHandler
Next step is creating Query Handler class that is suposed to return data to the client. This class should implements `IQueryHandler<in TAppContext, in TQuery, TResult>` where `TQuery` implements `IQuery<TResult>`. There is example query handler below:

```
using LeanCode.CQRS.Execution;
using LncdApp.DomainName.Contracts.Example;

namespace LncdApp.DomainName.Services.CQRS.Example;

public class ExampleQueryQH : IQueryHandler<DomainNameContext, ExampleQuery, QueryResultDTO>
{
    public Task<QueryResultDTO> ExecuteAsync(DomainNameContext context, ExampleQuery query)
    {
        return Task.FromResult(new QueryResultDTO
        {
            Greeting = $"Hello {query.Name}!",
        });
    }
}
```
Query handlers don't need seperate validation.

Best practices:
- Query handler names should consist of Query name and "QH". For example `ExampleQueryQH`.
