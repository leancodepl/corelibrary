# Local Execution

The LeanCode CoreLibrary utilizes ASP.NET Core to model the pipeline, which enables powerful HTTP execution of queries, commands and operations. Sometimes though, there is a need to run query/command/operation in-proc. This is enabled by so-called local execution of CQRS.

## Theory

Being able to call CQRS objects locally without going through HTTP pipeline is achieved by using a separate ASP.NET Core pipeline that is used with mocked [HttpContext]. This enables us to re-use all the middlewares created for normal HTTP execution (which applies to all middlewares provided by the CoreLibrary) without changes. This option also enables using the same handlers as the normal executions.

Nevertheless, the local execution only mimics the HTTP pipeline. Local execution does not involve proper request/response, thus it comes with limitations:

1. [HttpContext.Connection], [HttpContext.WebSockets], [HttpContext.Session] are represented by empty objects - report null/empty data and all actions either throw or are no-op,
2. [HttpContext.User] is passed by the caller (and can, but does not have to be, real),
3. [HttpContext.Response] is an empty object, meaning that it ignores any writes to it (both body and headers will be lost),
4. [HttpContext.Request] does not have any body, nor other HTTP metadata like path, method or so. The only thing that is implemented are headers, which can be passed when calling local execution.
5. The features provided by [DefaultHttpContext] are only partly available and we don't guarantee any feature to be available.

We found that, albeit some features that rely on HTTP will be ignored, most of the middlewares will be fully working.

Local executors preserve the semantics of HTTP calls, meaning that:

1. They are run in a separate DI scope,
2. They are stateless, and don't share anything with the parent call.

## Packages

| Package                  | Link        | Application in section |
| ------------------------ | ----------- | ---------------------- |
| LeanCode.CQRS.AspNetCore | [![NuGet version (LeanCode.CQRS.AspNetCore)](https://img.shields.io/nuget/vpre/LeanCode.CQRS.AspNetCore.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.CQRS.AspNetCore) | Configuration |

## Configuration

To use local execution, you have to explicitly register local executors (query/command/operation separately). This can be done by chaining calls to [AddCQRS(...)], specifying pipelines for local execution. For example:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    services
        .AddCQRS(TypesCatalog.Of<ExampleCommand>(), TypesCatalog.Of<ExampleHandler>())
        .WithLocalCommands(c => c.Secure().Validate().TranslateExceptions().CommitTransaction<CoreDbContext>())
        .WithLocalQueries(c => c.Validate().TranslateExceptions())
        .WithLocalOperations(c => c.Secure().TranslateExceptions().CommitTransaction<CoreDbContext>());
}
```

!!! tip
    There are keyed versions of `WithLocal*` calls. If you need to have different pipelines for different modules, you can register them under different keys.

## Usage

To call local objects, use `ILocalCommandExecutor`/`ILocalQueryExecutor`/`ILocalOperationExecutor`. All require you to pass:

1. Object that will be executed,
2. The [ClaimsPrincipal] that the action will be executed as,
3. And optionally a [IHeaderDictionary] with additional headers.

Executors return a value that corresponds to the result of the object being executed (e.g. `CommandResult` or query/operation result).

The example below uses query from [Query](../query/index.md) tutorial:

```csharp
public class ProcessProjectDataCH : ICommandHandler<ProcessProjectData>
{
    private readonly ILocalQueryExecutor queries;

    public UpdateProjectNameCH(ILocalQueryExecutor queries)
    {
        this.queries = queries;
    }

    public Task ExecuteAsync(HttpContext context, ProcessProjectData command)
    {
        // We call external (local) query to gather data. We call the query as the same user that calls this command.
        var projects = await queries.GetAsync(
            new AllProjects { NameFilter = "[IMPORTANT]" },
            context.User,
            context.RequestAborted);

        // We can do sth with `projects` here
    }
}
```

## Error reporting

All local executors handle results as follows:

1. Success (`200 OK`) and validation error (`422 Unprocessable Entity`) will be reported as the result of operation (meaning that validation errors in commands will be reported as `CommandResult`s),
2. Not authenticated calls (`401 Unauthorized`) will be reported as `UnauthenticatedCQRSRequestException`,
3. Not authorized calls (`403 Forbidden`) will be reported as `UnauthorizedCQRSRequestException`,
4. All other status codes will be reported as `UnknownStatusCodeException`.

This corresponds to the behavior of Remote CQRS calls.

[HttpContext]: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext
[HttpContext.Connection]: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.connection
[HttpContext.WebSockets]: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.websockets
[HttpContext.Session]: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.session
[HttpContext.User]: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.user
[HttpContext.Response]: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.response
[HttpContext.Request]: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.request
[DefaultHttpContext]: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.defaulthttpcontext
[AddCQRS(...)]: https://github.com/leancodepl/corelibrary/blob/HEAD/src/CQRS/LeanCode.CQRS.AspNetCore/ServiceCollectionCQRSExtensions.cs#L17
[ClaimsPrincipal]: https://learn.microsoft.com/en-us/dotnet/api/system.security.claims.claimsprincipal
[IHeaderDictionary]: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.iheaderdictionary
