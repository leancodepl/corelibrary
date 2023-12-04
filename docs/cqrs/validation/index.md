# Validation

To reject [commands] that have invalid data or that cannot be fulfilled (the state of the system disallows it), you should use command validators. A command validator is instantiated and run before command handler even sees the command (but, by default, after [authorization]) and can return error code along the error message, so the system has an opportunity to inform the client why the command is invalid. A validator is a class that implements the `ICommandValidator<TCommand>` interface. To simplify things, we use `FluentValidation` so it is only necessary to implement `AbstractValidator<TCommand>`, everything else is handled by infrastructure.

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| FluentValidation | [![NuGet version (LeanCode.CQRS.Validation.Fluent)](https://img.shields.io/nuget/vpre/FluentValidation.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/FluentValidation/11.8.0/) | Validation |
| LeanCode.CQRS.Validation | [![NuGet version (LeanCode.CQRS.Validation)](https://img.shields.io/nuget/vpre/LeanCode.CQRS.Validation.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.CQRS.Validation/8.0.2260-preview/) | `ICommandValidator` |
| LeanCode.CQRS.Validation.Fluent | [![NuGet version (LeanCode.CQRS.Validation.Fluent)](https://img.shields.io/nuget/vpre/LeanCode.CQRS.Validation.Fluent.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.CQRS.Validation.Fluent/8.0.2260-preview/) | `AbstractValidator` |

To validate example command introduced in [command] section, you can add following code:

```csharp
[ProjectIsOwned]
[AuthorizeWhenHasAnyOf(Auth.Roles.Employee)]
public class UpdateProjectName : ICommand, IProjectRelated
{
    public string ProjectId { get; set; }
    public string Name { get; set; }

    // Error codes are part of the contract
    public static class ErrorCodes
    {
        public const int InvalidName = 1;
        public const int ProjectIdInvalid = 2;
        public const int ProjectDoesNotExist = 3;
    }
}
```

```csharp
public class UpdateProjectNameCV : AbstractValidator<UpdateProjectName>
{
    public UpdateProjectNameCV(IRepository<Project, ProjectId> projects)
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithCode(UpdateProjectName.ErrorCodes.InvalidName);

        // . . .
    }

    // . . .
}
```

If you need complex validation logic that needs to access external state, use `MustAsync`/`CustomAsync` validators. For `CustomAsync` validators, you can use `AddValidationError` helper to specify the error code:

```csharp
public class UpdateProjectNameCV : AbstractValidator<UpdateProjectName>
{
    private readonly CoreDbContext dbContext;

    public UpdateProjectNameCV(CoreDbContext dbContext)
    {
        this.dbContext = dbContext;

        RuleFor(c => c.Name)
            .NotEmpty().WithCode(UpdateProjectName.ErrorCodes.InvalidName);

        RuleFor(c => c.ProjectId)
            .CustomAsync(CheckProjectDoesNotExistAsync);
    }

    private async Task CheckProjectExistsAsync(
        string ProjectId,
        ValidationContext<UpdateProjectName> ctx,
        CancellationToken cancellationToken
    )
    {
        if (!ProjectId.TryParse(ProjectId, out var parsedProjectId))
        {
            ctx.AddValidationError(
                "ProjectId is invalid.",
                UpdateProjectName.ErrorCodes.ProjectIdInvalid);

            return;
        }

        var exists = await dbContext.Projects.AnyAsync(
            p => p.Id == parsedProjectId,
            cancellationToken);

        if (!exists)
        {
            ctx.AddValidationError(
                $"Project with the id: {ProjectId} does not exist.",
                UpdateProjectName.ErrorCodes.ProjectDoesNotExist
            );
        }
    }
}
```

!!! tip
    More on ids can be found [here](../../domain/id/index.md).

[commands]: ../command/index.md
[command]: ../command/index.md
[authorization]: ../authorization/index.md
