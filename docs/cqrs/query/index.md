# Query

Query is just an class that implements the `IQuery<TResult>` interface (there's also non-generic `IQuery` interface but it shouldn't be used directly). The only generic parameter specifies the type that the query returns when executed. It should be a DTO (because most of the time it will be serialized). Queries get the data from the system but don't modify it.

## Contract

Consider the following query

```csharp
public class ProjectInfoDTO
{
    public string ProjectId { get; set; }
    public string Name { get; set; }
}

[AllowUnauthorized]
public class FindProjectsMatchingName : IQuery<List<ProjectInfoDTO>>
{
    public string NameFilter { get; set; }
}
```

It finds all the Projects that match the name filter (however we define the filter). It may be called anonymously and returns a list of `ProjectInfoDTO`s (we use a `List` instead of a `IList` or `IReadOnlyList` because of the DTO constraint; `List` is more DTO-ish than any interface).

## Handler

Query handlers execute queries. They should not have any side effects but can return data back to the client. Since they can return data to the client, they don't need separate [validation] (handler can do it internally). In query handlers you don't need to operate on aggregate level (as this is read-side and is relatively DDD-free) and are allowed to perform arbitrary SQL queries.

For the above query, you can have handler like this:

```csharp
public class FindProjectsMatchingNameQH
    : IQueryHandler<FindProjectsMatchingName, List<ProjectInfoDTO>>
{
    private readonly CoreDbContext dbContext;

    public FindProjectsMatchingNameQH(CoreDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<List<ProjectInfoDTO>> ExecuteAsync(
        HttpContext context,
        FindProjectsMatchingName query)
    {
        // Here, we use raw SQL but you are free
        // to use other mechanisms to get the data
        var filter = $"%{query.NameFilter.ToLower()}%";
        var results = await dbContext.QueryAsync<ProjectInfoDTO>(@"
            SELECT ""Id"" AS ""ProjectId"", ""Name"" FROM ""Projects""
            WHERE ""Name"" LIKE @filter",
            new { filter });
        return results.AsList();
    }
}
```

[validation]: ../validation/index.md
