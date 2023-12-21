# Query

Query is a class that implements the `IQuery<TResult>` interface (there's also non-generic `IQuery` interface but it shouldn't be used directly). The only generic parameter specifies the type that the query returns when executed. It should be a DTO (because most of the time it will be serialized). Queries get the data from the system but don't modify it.

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.Contracts | [![NuGet version (LeanCode.Contracts)](https://img.shields.io/nuget/vpre/LeanCode.Contracts.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.Contracts/2.0.0-preview.3/) | `IQuery` |
| LeanCode.CQRS.Execution | [![NuGet version (LeanCode.CQRS.Execution)](https://img.shields.io/nuget/vpre/LeanCode.CQRS.Execution.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.CQRS.Execution/8.0.2260-preview/) | `IQueryHandler` |

## Contract

Consider the query that finds all projects that match the name filter. It may be called anonymously and returns a list of `ProjectDTO`s (we use a `List` instead of a `IList` or `IReadOnlyList` because of the DTO constraint; `List` is more DTO-ish than any interface):

```csharp
namespace ExampleApp.Contracts.Projects;

[AllowUnauthorized]
public class AllProjects : IQuery<List<ProjectDTO>>
{
    public string? NameFilter { get; set; }
}

public class ProjectDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
}
```

!!! Remarks note
    - There's also non-generic `IQuery` interface but it shouldn't be used directly.
    - We use a `List` instead of a `IList` or `IReadOnlyList` because of the DTO constraint. `List` is more DTO-ish than any interface.

## Naming conventions

Queries are designed to retrieve information without altering the system's state. To maintain a clear and consistent naming convention, queries should possess names that directly indicate the type of information being requested, including the namespace as part of the contract. An effective approach is to use descriptive nouns or noun phrases within the designated namespace, exemplified by names like:

* `ExampleApp.Contracts.Projects.AllProjects`
* `ExampleApp.Contracts.Projects.ProjectById`
* `ExampleApp.Contracts.Employees.EmployeesInAssignment`

Query handlers should similarly be named in alignment with the associated query, appending the QH suffix within the respective namespace structure.

## Handler

Query handlers execute queries. They should not have any side effects but can return data back to the client. Since they can return data to the client, they don't need separate [validation] (it is rare to need validation; if it is needed, handlers can do it internally). In query handlers you don't need to operate on aggregate level (as this is read-side and is relatively DDD-free) and are allowed to perform arbitrary SQL queries.

For the above query, you can have handler like this:

```csharp
namespace ExampleApp.CQRS.Projects;

public class AllProjectsQH : IQueryHandler<AllProjects, List<ProjectDTO>>
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

[validation]: ../validation/index.md
