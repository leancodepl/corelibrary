# Writing commands

## Intro

Interacting with systems based with Core Library is based on a Command Query Responsibility Segregation ([CQRS](../basics/02_cqrs.md)) principle. As such, there are two main types of actions which can be performed in the system: command, and [query](./03_writing_queries.md) [^1]. In this guide we will focus on the former. A command is used to change the state of the system but it does not yield any results - it can be treated as write-only action.

Each command consists of three parts:

* Contract
* Command validator
* Command handler

## Contract

A contract, as the name suggests, defines the way a command can be invoked by the clients. Let's see how a contract for creating a project might look like.

```csharp
[AllowUnauthorized]
public class CreateProject : ICommand
{
    public string Name { get; set; }

    public static class ErrorCodes
    {
        public const int NameCannotBeEmpty = 1;
        public const int NameTooLong = 2;
    }
}
```

The contract for a command is simply a class implementing the `ICommand` interface. The fields of a contract class define how the body of an HTTP request should look like. In this example, an example of a valid request body would be:

```json
{
    "Name": "abc",
}
```

Notice the `ErrorCodes` class. These define possible validation errors which can be expected by the clients. Each error code should be a unique (in a given contract) integer.

Writing the contracts in that way makes it possible for web and mobile clients to generate their own version of contract classes with use of the Contract Generator which is also a part of CoreLibrary.

Also, a contract can have authorization attributes which define rules which have to be satisfied for an actor to be able to invoke the command. To learn more about authorizers, see [authorization](./0X_authorization.md).

## Command validator

Command validator's main role is to ensure that the command invoked by the user is valid. The command validator should check the structure of the command itself, for example, check for required fields, check whether passed strings do not exceed maximum length etc. Also the validator should check the business logic of the command for example, when editing a project, a command validator should check whether or not the project exists, see [more complex commands](#more-complex-commands) for an example. Let's see how a validator for `CreateProject` command might look like:

```csharp
public class CreateProjectCV : ContextualValidator<CreateProject>
{
    public CreateProjectCV()
    {
        RuleFor(cmd => cmd.Name)
            .NotEmpty()
            .WithCode(CreateProject.ErrorCodes.NameCannotBeEmpty)
            .MaximumLength(500)
            .WithCode(CreateProject.ErrorCodes.NameTooLong);
    }
}
```

A validator for a command should inherit from `ContextualValidator<T>` where `T` is a type of command contract. The validation in CoreLibrary builds upon [FluentValidation](https://docs.fluentvalidation.net/en/latest/), adding `WithCode()` method which is responsible for adding an error code to the response in case the validation fails. In the above example, trying to change a project name to one which is over 500 characters long would cause a command to fail and return an appropriate error code to the client.

## Command handler

> **Info :information_source:**
> A command validator and handler are usually put in a single file with a `CH` suffix, e.g. `CreateProjectCH.cs`.

Finally, we arrive at a command handler that is a part which actually executes the command. For the `CreateProject` command, a handler may look like this:

```csharp
public class CreateProjectCH : ICommandHandler<CoreContext, CreateProject>
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<CreateProjectCH>();

    private readonly IRepository<Project, SId<Project>> projects;

    public CreateProjectCH(IRepository<Project, SId<Project>> projects)
    {
        this.projects = projects;
    }

    public Task ExecuteAsync(CoreContext context, CreateProject command)
    {
        var project = Project.Create(command.Name);
        projects.Add(project);

        logger.Information("Project {ProjectId} added", project.Id);

        return Task.CompletedTask;
    }
}
```

> **Warning :warning:**
> According to DDD rules, a single command should only modify a single aggregate as an aggregate should be a transaction boundary in the system. If there is a need to modify multiple aggregates as a part of a single operation, raise events and modify all aggregates one by one in event handlers.

The logic of the handler is straightforward - a new project with a given name is created and added to `projects` repository. The `IRepository` interface is a part of Core Library which allows to abstract the logic of persisting domain entities. A repository (and other services) should be injected into a command handler using dependency injection. Also, notice that the `Add` method used to persist newly created project is synchronous. The actual asynchronous operation in the database is performed by `StoreAndPublishEvents()` element in the pipeline, see [pipeline](./0X_pipeline.md) for more info. A basic repository for projects allowing for operations on database may look like this:

```csharp
public class ProjectsRepository : EFRepository<Project, SId<Project>, CoreDbContext>
{
    public ProjectsRepository(CoreDbContext dbContext)
        : base(dbContext) { }

    public override Task<Project?> FindAsync(SId<Project> id, CancellationToken cancellationToken = default)
    {
        return DbSet.AsTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken)!;
    }
}

```

Here `EFRepository` is a class from Core Library containing default implementations for `Add`, `Delete` and `Update` methods which can be used to alter the contents of the database. Usually `FindAsync` method can be implemented as a simple retrieval of an entity based on Id but more complicated operations can also be done in the method if they are needed.

> **Warning :warning:**
> If an implemented command does not work, make sure that all referenced types are properly registered in the dependency injection container.

## More complex commands

The command for creating a project is a really basic one - it only takes a name for new project and all of the validation can be done without checking the state of the domain. We will now show a slightly more complex command which will be responsible for adding assignments to existing projects.

```csharp
[AllowUnauthorized]
public class AddAssignmentsToProject : ICommand
{
    public string ProjectId { get; set; }
    public List<AssignmentDTO> Assignments { get; set; }

    public static class ErrorCodes
    {
        public const int ProjectDoesNotExist = 1;
        public const int AssignmentsCannotBeNull = 2;
        public const int AssignmentsCannotBeEmpty = 3;
    }
}

public class AssignmentDTO
{
    public string Name { get; set; }

    public class ErrorCodes
    {
        public const int NameCannotBeEmpty = 101;
        public const int NameTooLong = 102;
    }
}
```

Notice the use of `AssignmentDTO` class. DTO classes can be defined as part of contracts and matching classes will be generated for web and mobile clients. Also, notice that in `AssignmentDTO` we used error codes starting from 101 to avoid clashes with error codes in `AddAssignmentsToProject`. Now, let's move onto validating the command:

```csharp
public class AddAssignmentsToProjectCV : ContextualValidator<AddAssignmentsToProject>
{
    public AddAssignmentsToProjectCV()
    {
        RuleFor(cmd => cmd.Assignments)
            .NotNull()
            .WithCode(AddAssignmentsToProject.ErrorCodes.AssignmentsCannotBeNull)
            .NotEmpty()
            .WithCode(AddAssignmentsToProject.ErrorCodes.AssignmentsCannotBeEmpty);

        RuleForEach(cmd => cmd.Assignments)
            .ChildRules(child => child.RuleFor(c => c).SetValidator(new AssignmentDTOValidator()));

        RuleForAsync(cmd => cmd, CheckProjectExistsAsync)
            .Equal(true)
            .WithCode(AddAssignmentsToProject.ErrorCodes.ProjectDoesNotExist)
            .WithMessage("A project with given Id does not exist.");
    }

    private Task<bool> CheckProjectExistsAsync(IValidationContext ctx, AddAssignmentsToProject command)
    {
        if (!SId<Project>.TryParse(command.ProjectId, out var projectId))
        {
            return Task.FromResult(false);
        }

        var appContext = ctx.AppContext<CoreContext>();
        var dbContext = ctx.GetService<CoreDbContext>();

        return dbContext.Projects.AnyAsync(p => p.Id == projectId, appContext.CancellationToken);
    }
}

public class AssignmentDTOValidator : AbstractValidator<AssignmentDTO>
{
    public AssignmentDTOValidator()
    {
        RuleFor(dto => dto.Name)
            .NotEmpty()
            .WithCode(AssignmentDTO.ErrorCodes.NameCannotBeEmpty)
            .MaximumLength(500)
            .WithCode(AssignmentDTO.ErrorCodes.NameTooLong);
    }
}
```

There are two important things to note here. First one is the use of a custom validator for validating `AssignmentDTO`. Writing such validators may allow for reusing the validation logic in commands using the same DTO classes. The second one is performing asynchronous validation - to check whether a project we want to add the tasks for exists, we perform a database query inside the `CheckProjectExistsAsync` method. This allows to check that the command to be executed does not violate the logic of the business domain.

Now, let's implement the handler for the command:

```csharp
public class AddAssignmentsToProjectCH : ICommandHandler<CoreContext, AddAssignmentsToProject>
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<AddAssignmentsToProjectCH>();

    private readonly IRepository<Project, SId<Project>> projects;

    public AddAssignmentsToProjectCH(IRepository<Project, SId<Project>> projects)
    {
        this.projects = projects;
    }

    public async Task ExecuteAsync(CoreContext context, AddAssignmentsToProject command)
    {
        var project = await projects.FindAndEnsureExistsAsync(
            SId<Project>.From(command.ProjectId),
            context.CancellationToken
        );
        project.AddAssignments(command.Assignments.Select(a => a.Name));

        projects.Update(project);

        logger.Information(
            "{AssignmentCount} assignments added to project {ProjectId}.",
            command.Assignments.Count,
            project.Id
        );
    }
}
```

What happens is as follows: we retrieve the existing project via `projects` repository, add assignments to it and update it in the repository. Again, please notice that the `Update` method is called synchronously - the actual saving of the changes in the database is performed as a part of request pipeline.

[^1]: There is also a third type of action called 'operation' which can both modify object and return data. It should only be used out of necessity as it is a violation of CQRS principle.
