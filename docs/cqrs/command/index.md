# Command

Command is just an class that implements the `ICommand` interface. Commands are used to execute an action that modifies data. Commands are validated, and if they pass validation, they should succeed. Commands do not return any value. This makes them quite constrained, yet reasoning is much easier.

## Contract

Consider the following command:

```csharp
[AuthorizeWhenHasAnyOf(Permission.CreateProject)]
public class CreateProject : ICommand
{
    public string ProjectId { get; set; }
    public string Name { get; set; }

    public static class ErrorCodes
    {
        public const int InvalidName = 1;
        public const int ProjectIdInvalid = 2;
        public const int ProjectAlreadyExists = 3;
    }
}
```

that creates a new project. Caller of the command is required to have the `CreateProject` permission.

> **Tip:** More on authorization and permissions can be found [here](../authorization/index.md) and on error codes and validation [here](../validation/index.md).

## Handler

For the above command, you can have handler like this:

```csharp
public class CreateProjectCH : ICommandHandler<CreateProject>
{
    private readonly IRepository<Project, ProjectId> projects;

    public CreateProjectCH(IRepository<Project, ProjectId> projects)
    {
        this.projects = projects;
    }

    public Task ExecuteAsync(HttpContext context, CreateProject command)
    {
        // context.EmployeeId() is an extension method defined elsewhere
        var project = Project.Create(
            ProjectId.Parse(command.ProjectId),
            command.Name,
            context.EmployeeId());

        // We only notify the repository that this is new entity,
        // we let other part of the code commit the database transaction
        projects.Add(project);

        // `Execute` operations are async by nature, but here we don't need it
        return Task.CompletedTask;
    }
}
```

> **Tip:** More on ids can be found [here](../../domain/id/index.md).

As you can see, the command handler is really simple - it just converts the command into new [aggregate], tracking who owns the project (`EmployeeId` - they are the ones that have `CreateProject` permission). That does not mean this is the only responsibility of the handlers (it's just an example), but there are some guidelines related to them:

1. Keep them simple and testable, do not try to model whole flows with a single command.
2. Commands should rely on aggregates to gather the data (try not to use queries inside command handlers).
3. Commands should modify just a single aggregate (try to `Add`/`Update`/`Delete` at most once).
4. If the business process requires to modify multiple aggregates, try to use [events] (but don't over-engineer).
5. If that does not help, modify/add/delete multiple aggregates.
6. Do not throw exceptions from inside commands. The client will receive generic error (`500 Internal Server Error`). Do it only as a last resort.
7. Database transaction will be commited at the end of the [pipeline] (assuming [CommitTransaction] pipeline element was added), so it's not recommended to commit it inside query handler as it may make serialized [events] inconsistent with the entity.

[aggregate]: ../../domain/aggregate/index.md
[events]: ../../domain/domain_event/index.md
[pipeline]: ../pipeline/index.md
[CommitTransaction]: https://github.com/leancodepl/corelibrary/blob/v8.0-preview/src/CQRS/LeanCode.CQRS.MassTransitRelay/MassTransitRelayApplicationBuilderExtensions.cs#L9
