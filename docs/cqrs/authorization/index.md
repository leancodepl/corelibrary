# Authorization

Each command and query has to be authorized or must explicitly opt-out of authorization (it's enforced using Roslyn analyzers). You can specify which authorizer to use using the `AuthorizeWhen` attribute and custom `ICustomAuthorizer`. Opting-out is done using the `AllowUnauthorized` attribute. There is a predefined authorizer that uses role- and permission-based authorization. You can specify which permissions to enforce using `AuthorizeWhenHasAnyOf` and configure the role-to-permission relationship using `IRoleRegistrations`.

If multiple `AuthorizeWhen` attributes are specified, **all** authorization rules must pass.

An authorizer is a class that implements the `ICustomAuthorizer` interface or derives from one of the `CustomAuthorizer` base classes. It has access to both context and command/query. Command/query type doesn't need to be exact, it just has to be coercible to the specified type (`CustomAuthorizer` casts objects to the types internally). Therefore, if you want to use the same authorizer for many commands/queries, you can use base classes or interfaces and implement the authorizer for them.

Example authorizer, along with the (not required, but convenient) plumbing:

```csharp


// Object that use `ProjectIsOwned` attribute must implement this interface
public interface IProjectRelated
{
    string ProjectId { get; }
}

// Authorizer marker
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
    private readonly IRepository<Project, ProjectId> Projects;

    public ProjectIsOwnedAuthorizer(IRepository<Project, ProjectId> Access)
    {
        this.Access = Access;
    }

    protected override async Task<bool> CheckIfAuthorizedAsync(
        HttpContext context,
        IProjectRelated obj)
    {
        var project = await Access.FindAsync(new(obj.ProjectId), context.RequestAborted);
        return project.OwnerId == context.GetUserId();
    }
}
```

Both queries, commands and operations can (and should!) be behind authorization. By default, authorization is run before validation so the object that the command/query/operation is pointing at might not exist.

> **Tip:** You can implement your own authorization and use it with LeanCode CoreLibrary authorizers. To see how you can implement authorization using Ory Kratos and LeanCode CoreLibrary see here. <!-- TODO: add link to Ory Kratos page -->
