# Authorization

Each command and query has to be authorized or must explicitly opt-out of authorization (it's enforced using Roslyn analyzers). You can specify which authorizer to use using the `AuthorizeWhen` attribute and custom `ICustomAuthorizer`. Opting-out is done using the `AllowUnauthorized` attribute. There is a predefined authorizer that uses role- and permission-based authorization. You can specify which permissions to enforce using `AuthorizeWhenHasAnyOf` and configure the role-to-permission relationship using `IRoleRegistrations`.

If multiple `AuthorizeWhen` attributes are specified, **all** authorization rules must pass.

An authorizer is a class that implements the `ICustomAuthorizer` interface or derives from one of the `CustomAuthorizer` base classes. It has access to both context and [command]/[query]/[operation]. [Command]/[query]/[operation] type doesn't need to be exact, it just has to be coercible to the specified type (`CustomAuthorizer` casts objects to the types internally). Therefore, if you want to use the same authorizer for many [commands]/[queries]/[operations], you can use base classes or interfaces and implement the authorizer for them.

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.Contracts | [![NuGet version (LeanCode.Contracts)](https://img.shields.io/nuget/vpre/LeanCode.Contracts.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.Contracts) | Default authorizers |
| LeanCode.CQRS.Security | [![NuGet version (LeanCode.CQRS.Security)](https://img.shields.io/nuget/vpre/LeanCode.CQRS.Security.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.CQRS.Security) | Configuration, custom authorizers |

## AuthorizeWhenHasAnyOf

The `AuthorizeWhenHasAnyOf` attribute, found in `LeanCode.Contracts.Security`, has default authorization implementation. Upon its application, the `CheckIfAuthorizedAsync` method from the `DefaultPermissionAuthorizer` class is invoked to check whether the user possesses adequate permissions.

`CheckIfAuthorizedAsync` method employs the `RoleRegistry` class to retrieve roles within the system. To integrate roles and ensure proper functionality, a class implementing `IRoleRegistration` must be added to the Dependency Injection (DI) container. The first argument in the `Role` constructor represents the role, and subsequent arguments denote permissions passed as `params`:

```csharp
internal class AppRoles : IRoleRegistration
{
    public IEnumerable<Role> Roles { get; } = new[]
    {
        new Role("employee", "employee"),
        new Role("admin", "admin"),
    };
}
```

To register this class in the DI container, include the following code in the `ConfigureServices` method:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    // . . .

    services.AddSingleton<LeanCode.CQRS.Security.IRoleRegistration, AppRoles>();

    // . . .
}

```

Following this registration, the `AuthorizeWhenHasAnyOf` attribute can be utilized as demonstrated below, checking if a user possesses the `employee` role within the `ClaimsPrincipal`:

```csharp
[AuthorizeWhenHasAnyOf("employee")]
public class Projects : IQuery<List<ProjectDTO>>
{
    public string? NameFilter { get; set; }
}
```

This attribute enables the enforcement of access control based on specified roles.

## AllowUnauthorized
<!-- TODO: add link to analyzers section when it's ready -->
All [query], [command] and [operation] require usage of authorization attribute (which can enforced by Roslyn analyzers). To bypass the authorization requirements, developers can employ the `AllowUnauthorized` attribute as demonstrated below to skip authorization entirely:

```csharp
[AllowUnauthorized]
public class Projects : IQuery<List<ProjectDTO>>
{
    public string? NameFilter { get; set; }
}
```

## Custom authorizers

Other than `AuthorizeWhenHasAnyOf` and `AllowUnauthorized` attributes which have default implementations custom authorizers can be defined. Here is an example along with the (not required, but convenient) plumbing:

```csharp
// Object that use `ProjectIsOwned` attribute must implement this interface.
public interface IProjectRelated
{
    string ProjectId { get; }
}

// A marker for authorization required for DI resolution in `CQRSSecurityMiddleware`.
public interface IProjectIsOwned { }

[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
sealed class ProjectIsOwnedAttribute : AuthorizeWhenAttribute
{
    public ProjectIsOwned()
        : base(typeof(IProjectIsOwned))
    { }
}
```

Sample usage:

```csharp
[ProjectIsOwned]
public class UpdateProjectName : ICommand, IProjectRelated
{
    public string ProjectId { get; set; }
    public string Name { get; set; }
}

public class ProjectIsOwnedAuthorizer
    : CustomAuthorizer<IProjectRelated>, IProjectIsOwned
{
    private readonly IRepository<Project, ProjectId> projects;

    public ProjectIsOwnedAuthorizer(IRepository<Project, ProjectId> projects)
    {
        this.projects = projects;
    }

    protected override async Task<bool> CheckIfAuthorizedAsync(
        HttpContext context,
        IProjectRelated obj)
    {
        var project = await projects.FindAsync(new(obj.ProjectId), context.RequestAborted);

        if (project is null)
        {
            // If no project is found we let validation handle it.
            return true;
        }

        return project.OwnerId == context.GetEmployeeId();
    }
}
```

All [queries], [commands] and [operations] can (and should!) be behind authorization. If pipeline is configured as below, authorization is run before validation so the object that the [command]/[query]/[operation] is pointing at might not exist and we let validation handle this case.

```csharp
    protected override void ConfigureApp(IApplicationBuilder app)
    {
        // . . .
        app.UseEndpoints(endpoints =>
            {
                endpoints.MapRemoteCQRS(
                    "/api",
                    cqrs =>
                    {
                        // . . .

                        cqrs.Commands = c =>
                            c.CQRSTrace()
                            // Authorization is before validation.
                            .Secure()
                            .Validate()
                            .CommitTransaction<CoreDbContext>()
                            .PublishEvents();

                        // . . .
                    }
                );
            });
    }
```

!!! tip
    You can implement your own authorization and use it with LeanCode CoreLibrary authorizers. To see how you can implement authorization using Ory Kratos and LeanCode CoreLibrary see [here](../../external_integrations/authorization_ory_kratos/index.md).

[query]: ../query/index.md
[command]: ../command/index.md
[operation]: ../operation/index.md
[commands]: ../command/index.md
[queries]: ../query/index.md
[operations]: ../operation/index.md
