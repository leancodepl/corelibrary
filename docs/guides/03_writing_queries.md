# Writing queries

## Intro

In the previous guide, we described how to write commands - actions which modify the data in the system. This guide shows how queries can be written. Contrary to commands, queries are responsible for retrieving data without modifying any objects and should be considered read only.

Each query consists of two parts:

* Contract
* Query handler

This is analogous to how [commands](./02_writing_commands.md) are implemented, except a validator.

## Contract

As it is the case with command contracts, a query contract defines the parameters passed to a query as well as the structure of the response. We will show a simple query for retrieving a list of projects.

```csharp
[AllowUnauthorized]
public class AllProjects : IQuery<List<ProjectDTO>>
{
    public bool SortByNameDescending { get; set; }
}

public class ProjectDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
}
```

A contract for a query is defined by creating a class implementing the `IQuery<T>` interface. The `T` is the result type of the query, which is serialized in the HTTP response after the query has been executed. Also, web and mobile clients can use the Contracts Generator to generate classes corresponding to defined queries and their result types.

Fields in a query class represent parameters which can be passed in a request to alter the query results. In the above example there is a single query parameter of boolean type `SortByNameDescending`, which will be used to determine sorting order of the results. Queries can contain more fields used for filtering, sorting, pagination etc. of the results.

## Handler

A handler for a command is a class implementing `IQueryHandler<TContext, TQuery, TResult>` interface where `TQuery` is the contract type and `TResult` is the result type as defined in the contract. In our example, a query handler might look like this:

```csharp
public class AllProjectsQH : IQueryHandler<CoreContext, AllProjects, List<ProjectDTO>>
{
    private readonly CoreDbContext dbContext;

    public AllProjectsQH(CoreDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<List<ProjectDTO>> ExecuteAsync(CoreContext context, AllProjects query)
    {
        var q = dbContext.Projects.Select(p => new ProjectDTO
        {
            Id = p.Id,
            Name = p.Name,
        });

        q = query.SortByNameDescending ? q.OrderByDescending(p => p.Name) : q.OrderBy(p => p.Name);

        return q.ToListAsync(context.CancellationToken);
    }
}
```

The `dbContext` used to query the database for data is passed in the constructor to be injected via Dependency Injection. Also, notice how `SortByNameDescending` parameter from the query is used to determine the sorting order of the result. Also, by convention we return a default value when requested data doesn't exist instead of throwing an error.

> **Info :information_source:**
> Generally, it is a good practice to keep the number of database calls in a query handler to a minimum, trying to limit them to at most one.
