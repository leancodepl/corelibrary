# Query

Query is just a class that implements the `IQuery<TResult>` interface (there's also non-generic `IQuery` interface but it shouldn't be used directly). The only generic parameter specifies the type that the query returns when executed. It should be a DTO (because most of the time it will be serialized). Queries get the data from the system but don't modify it.

## Contract

Consider the query that finds all projects that match the name filter. It may be called anonymously and returns a list of `ProjectDTO`s (we use a `List` instead of a `IList` or `IReadOnlyList` because of the DTO constraint; `List` is more DTO-ish than any interface):

```csharp
public class ProjectDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
}

[AllowUnauthorized]
public class Projects : IQuery<List<ProjectDTO>>
{
    public string? NameFilter { get; set; }
}
```

## Handler

Query handlers execute queries. They should not have any side effects but can return data back to the client. Since they can return data to the client, they don't need separate [validation] (handler can do it internally). In query handlers you don't need to operate on aggregate level (as this is read-side and is relatively DDD-free) and are allowed to perform arbitrary SQL queries.

For the above query, you can have handler like this:

```csharp
public class ProjectsQH : IQueryHandler<Projects, List<ProjectDTO>>
{
    private readonly CoreDbContext dbContext;

    public ProjectsQH(CoreDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<List<ProjectDTO>> ExecuteAsync(
        HttpContext context,
        Projects query)
    {
        // Here, we use Entity Framework but you are free
        // to use other mechanisms to get the data
        var dbQuery = dbContext.Projects.AsQueryable();

        if (!string.IsNullOrEmpty(query.NameFilter))
        {
            dbQuery = dbQuery.Where(p => p.Name.Contains(query.NameFilter));
        }

        return await dbQuery
            .Select(p => new ProjectDTO
            {
                Id = p.Id,
                Name = p.Name
            })
            .ToListAsync(context.RequestAborted);
    }
}
```

## Naming conventions

Queries are designed to retrieve information without altering the system's state. To maintain a clear and consistent naming convention, queries should possess names that directly indicate the type of information being requested. An effective approach is to use descriptive nouns or noun phrases, exemplified by names like `Projects`, `ProjectById`, or `EmployeesInAssignment`. Query handlers should be named in alignment with the associated query, appending the `QH` suffix.

[validation]: ../validation/index.md
