# Validation

To reject [commands] that have invalid data or that cannot be fulfilled (the state of the system disallows it), you should use command validators. A command validator is instantiated and run before command handler even sees the command (but, by default, after [authorization]) and can return error code along the error message, so the system has an opportunity to inform the client why the command is invalid. A validator is a class that implements the `ICommandValidator<TCommand>` interface. To simplify things, we use `FluentValidation` so it is only necessary to implement `AbstractValidator<TCommand>`, everything else is handled by infrastructure.

To validate example command introduced in [command] section, you can use something like:

```csharp
public class CreateProject : ICommand
{
    public string ProjectId { get; set; }
    public string Name { get; set; }

    // Error codes are part of the contract
    public static class ErrorCodes
    {
        public const int InvalidName = 1;
        public const int ProjectIdInvalid = 2;
        public const int ProjectAlreadyExists = 3;
    }
}

public class CreateProjectCV : AbstractValidator<CreateProject>
{
    private readonly IRepository<Project, ProjectId> projects;

    public CreateProjectCV(IRepository<Project, ProjectId> projects)
    {
        this.projects = projects;

        RuleFor(c => c.Name)
            .NotEmpty().WithCode(CreateProject.ErrorCodes.InvalidName);

        RuleFor(c => c.ProjectId)
            .CustomAsync(CheckProjectDoesNotExistAsync);
    }

    private async Task CheckProjectDoesNotExistAsync(
        string ProjectId,
        ValidationContext<CreateProject> ctx,
        CancellationToken ct
    )
    {
        if (!ProjectId.TryParse(ProjectId, out var parsedProjectId))
        {
            ctx.AddValidationError(
                "ProjectId is invalid.",
                CreateProject.ErrorCodes.ProjectIdInvalid);

            return;
        }

        var existing = await projects.FindAsync(parsedProjectId, ct);

        if (existing is not null)
        {
            ctx.AddValidationError(
                $"Project with the ID {ProjectId} already exists.",
                CreateProject.ErrorCodes.ProjectAlreadyExists
            );
        }
    }
}
```

> **Tip:** More on ids can be found [here](../../domain/id/index.md).

If you need complex validation logic that needs to access external state, use `MustAsync`/`CustomAsync` validators. For `CustomAsync` validators, you can use `AddValidationError` helper to specify the error code.

[commands]: ../command/index.md
[command]: ../command/index.md
[authorization]: ../authorization/index.md
