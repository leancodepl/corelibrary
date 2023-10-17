# Command

Command is just a class that implements the `ICommand` interface. Commands are used to execute an action that modifies data. Commands are validated, and if they pass validation, they should succeed. Commands do not return any value. This makes them quite constrained, yet reasoning is much easier.

## Contract

Consider the command that updates name of the `Project` (caller of the command is required to have `Employee` role and own project).

```csharp
[ProjectIsOwned]
[AuthorizeWhenHasAnyOf(Auth.Roles.Employee)]
public class UpdateProjectName : ICommand, IProjectRelated
{
    public string ProjectId { get; set; }
    public string Name { get; set; }

    public static class ErrorCodes
    {
        public const int InvalidName = 1;
        public const int ProjectIdInvalid = 2;
        public const int ProjectDoesNotExist = 3;
    }
}
```

> **Tip:** More on authorization and permissions can be found [here](../authorization/index.md) and on error codes and validation [here](../validation/index.md).

## Handler

For the above command, you can have handler like this:

```csharp
public class UpdateProjectNameCH : ICommandHandler<UpdateProjectName>
{
    private readonly IRepository<Project, ProjectId> projects;

    public UpdateProjectNameCH(IRepository<Project, ProjectId> projects)
    {
        this.projects = projects;
    }

    public Task ExecuteAsync(HttpContext context, UpdateProjectName command)
    {
        // We find a project with specified Id assuming that it exists,
        // it should be the responsibility of validation to ensure
        // that project exists.
        var project = await projects.FindAndEnsureExistsAsync(
            ProjectId.Parse(command.ProjectId),
            context.RequestAborted);

        project.UpdateName(command.Name);

        // We only notify the repository that entity has been updated,
        // we let other part of the code commit the database transaction.
        projects.Update(project);
    }
}
```

> **Tip:** More on ids can be found [here](../../domain/id/index.md) and on validation [here](../validation/index.md).

As you can see, the command handler is really simple - it just finds project with specified `ProjectId` and updates it's name. That does not mean this is the only responsibility of the handlers (it's just an example), but there are some guidelines related to them:

1. Keep them simple and testable, do not try to model whole flows with a single command.
2. Commands should rely on [aggregates] to gather the data (try not to use queries inside command handlers).
3. Commands should modify just a single aggregate (try to `Add`/`Update`/`Delete` at most once).
4. If the business process requires to modify multiple aggregates, try to use [events] (but don't over-engineer).
5. If that does not help, modify/add/delete multiple aggregates.
6. Do not throw exceptions from inside commands. The client will receive generic error (`500 Internal Server Error`). Do it only as a last resort.
7. Database transaction will be commited at the end of the [pipeline] (assuming [CommitTransaction] pipeline element was added), so it's not recommended to commit it inside query handler as it may make serialized [events] inconsistent with the entity.

[aggregates]: ../../domain/aggregate/index.md
[events]: ../../domain/domain_event/index.md
[pipeline]: ../pipeline/index.md
[CommitTransaction]: https://github.com/leancodepl/corelibrary/blob/v8.0-preview/src/CQRS/LeanCode.CQRS.MassTransitRelay/MassTransitRelayApplicationBuilderExtensions.cs#L9
