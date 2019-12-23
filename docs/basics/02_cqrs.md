CQRS
====

We use CQRS in almost every project. We try to stick to it as much as possible because it allows us to easily construct our APIs (using RemoteCQRS) and makes our code maintainable. Here, we present a short description on what CQRS is and how we designed it.

This is not an extensive CQRS description. :)

## Basics

Command-Query Responsibility Segregation promotes hard split between parts of code (classes/objects) that read data (query) and the ones that modify and write the data (command). It also separates the payload (e.g. object id, user name) from the code that executes the query/command.

We went a step further and used commands and queries as our API (client-facing, but clients are controlled by us). This forced us to extend the concept of command/query with the concept of context. The context describes an environment the command/query handler operates in. It is managed and constructed by the infrastructure and isn't part of command/query. This prevents us from assigning the data directly to command/query object and allows to get it from side channel (e.g. cookies, HTTP headers).

### Command

Command is just an object that implements the `ICommand` interface. Commands are used to execute an action that modifies data. Commands are validated, and if they pass validation, they should succeed. Commands do not return any value. This makes them quite constrained, yet reasoning is much easier.

Consider the following command:
```csharp
[AuthorizeWhenHasAnyOf(Permission.CreateDish)]
public class CreateDish : ICommand
{
    public Guid DishId { get; set; }
    public string Name { get; set; }
}
```
that creates a new dish. Caller of the command is required to have the `CreateDish` permission (more on authorization and permissions later).

### Query

Query is just an object that implements the `IQuery<TResult>` interface (there's also non-generic `IQuery` interface but it shouldn't be used directly). The only generic parameter specifies the type that the query returns when executed. It should be a DTO (because most of the time it will be serialized). Queries get the data from the system but don't modify it.

Consider the following query
```csharp
public class DishInfoDTO
{
    public Guid DishId { get; set; }
    public string Name { get; set; }
}

[AllowUnauthorized]
public class FindDishesMatchingName : IQuery<List<DishInfoDTO>>
{
    public string NameFilter { get; set; }
}
```

It finds all the dishes that match the name filter (however we define the filter). It may be called anonymously and returns a list of `DishInfoDTO`s (we use a `List` instead of a `IList` or `IReadOnlyList` because of the DTO constraint; `List` is more DTO-ish than any interface).

## Implementation

Commands and queries define the API surface but they aren't the actual code that will be executed. That's the reason why there are command and query handlers.

For each command/query there must be exactly one handler. Handler is just a simple class that implements `ICommandHandler<TContext, TCommand>` or `IQueryHandler<TContext, TQuery, TResult>` interface. It's responsibility is to fulfill the premise that the command has or provide data for the query.

### Command handlers

For the above command, you can have handler like this:

```csharp
public class CreateDishCH : ICommandHandler<AppContext, CreateDish>
{
    private readonly IRepository<Dish> dishes;

    public CreateDishCH(IRepository<Dish> dishes)
    {
        this.dishes = dishes;
    }

    public Task ExecuteAsync(AppContext context, CreateDish command)
    {
        var dish = Dish.Create(command.DishId, command.Name, context.UserId);
        return dishes.AddAsync(dish);
    }
}
```

As you can see, the command handler is really simple - it just converts the command into new aggregate, tracking who owns the dish (`UserId` - they are the ones that have `CreateDish` permission). That does not mean this is the only responsibility of the handlers (it's just an example), but there are some guidelines related to them:

 1. Keep them simple and testable, do not try to model whole flows with a single command,
 2. Commands should rely on aggregates to gather the data (try not to use queries inside command handlers),
 3. Commands should modify just a single aggregate (try to `AddAsync`/`UpdateAsync`/`DeleteAsync` at most once),
 4. If the business process requires to modify multiple aggregates, try to use events and event handlers (but don't over-engineer),
 5. If that does not help, modify/add/delete multiple aggregates,
 6. Do not throw exceptions from inside commands. The client will receive generic error (`500 Internal Server Error`). Do it only as a last resort.

#### Validation
To reject commands that have invalid data or that cannot be fullfilled (the state of the system disallows it), you should use command validators. A command validator is instantiated and run before command handler even sees the command (but, by default, after authorization) and can return error code along the error message, so the system has an opportunity to inform the client why the command is invalid. A validator is a class that implements the `ICommandValidator<TAppContext, TCommand>` interface. To simplify things, we use `FluentValidation` so it is only necessary to implement `AbstractValidator<TCommand>`, everything else is handled by infrastructure.

To validate the command above, you can use something like

```csharp
// `ContextualValidator` is a simple extension to `AbstractValidator` that provides `RuleForAsync`
public class CreateDishCV : ContextualValidator<CreateDish>
{
    public CreateDishCV()
    {
        this.CascadeMode = CascadeMode.StopOnFirstFailure;

        this.RuleFor(c => c.Name)
            .NotNull().WithCode(CreateDish.ErrorCodes.InvalidName)
            .NotEmpty().WithCode(CreateDish.ErrorCodes.InvalidName);

        this.RuleForAsync(
            c => c.DishId,
            // FluentValidation validators are singletons so we need to use service locator :(
            (ctx, id) => ctx.GetService<IRepository<Dish>>().FindAsync(id))
            .Null().WithCode(CreateDish.ErrorCodes.DishAlreadyExists);
    }
}

public class CreateDish : ICommand
{
    public Guid DishId { get; set; }
    public string Name { get; set; }

    // Error codes are part of the contract
    public static class ErrorCodes
    {
        public const int InvalidName = 1;
        public const int DishAlreadyExists = 2;
    }
}
```

Besides normal `RuleFor`s, there are `RuleForAsync`s. They are used to access external objects based on the command (and/or context) itself. You can't take dependencies in constructor, because FluentValidator-based validators are singletons (this is an optimization - parsing expressions in RuleFor is quite heavy and we want to do it only once per command), so we fall back to service locator-based approach and resolve services in validator code (this isn't the best approach, but we don't have a better one yet).

### Query handlers

Query handlers execute queries. They should not have any side effects but can return data back to the client. Since they can return data to the client, they don't need separate validation (handler can do it internally). In query handlers you don't need to operate on aggregate level (as this is read-side and is relatively DDD-free) and are allowed to perform arbitrary SQL queries (it is even recommended to not rely on EF or other ORM and go straight to Dapper).

Example query handler:
```csharp
public class FindDishesMatchingNameQH : IQueryHandler<AppContext, FindDishesMatchingName, List<DishInfoDTO>>
{
    private readonly CoreDbContext dbContext;

    public FindDishesMatchingNameQH(CoreDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<List<DishInfoDTO>> ExecuteAsync(AppContext context, FindDishesMatchingName query)
    {
        var filter = $"%{query.NameFilter.ToLower()}%";
        var results = await dbContext.QueryAsync<DishInfoDTO>(@"
            SELECT [Id] AS [DishId], [Name] FROM [Dishes]
            WHERE [Name] LIKE @filter",
            new { filter });
        return results.AsList();
    }
}
```

### Authorization
Each command and query has to be authorized or must explicitly opt-out of authorization (we enforce it using Roslyn analyzers). You can specify which authorizer to use using the `AuthorizeWhen` attribute and custom `ICustomAuthorizer`. Opting-out is done using the `AllowUnauthorized` attribute. There is a predefined authorizer that uses role- and permission-based authorization. You can specify which permissions to enforce using `AuthorizeWhenHasAnyOf` and configure the role-to-permission relationship using `IRoleRegistrations`.

If multiple `AuthorizeWhen` attributes are specified, **all** authorization rules must pass.

An authorizer is a class that implements the `ICustomAuthorizer` interface or derives from one of the `CustomAuthorizer` base classes. It has access to both context and command/query. Command/query type doesn't need to be exact, it just has to be coercible to the specified type (`CustomAuthorizer` casts objects to the types internally). Therefore, if you want to use the same authorizer for many commands/queries, you can use base classes or interfaces and implement the authorizer for them.

Example authorizer, along with the (not required, but convenient) plumbing:
```csharp
// Authorizer marker
public interface IsDishOwned
{ }

// Object that use `DishIsOwned` attribute must implement this interface
public interface IAmDish
{
    Guid DishId { get; }
}

[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
sealed class DishIsOwnedAttribute : AuthorizeWhenAttribute
{
    public DishIsOwned()
        : base(typeof(IsDishOwned))
    { }
}
```

Sample usage:
```csharp
[DishIsOwned]
public class UpdateDishName : ICommand, IAmDish
{
    public Guid DishId { get; set; }
    public string Name { get; set; }
}

public class DishOwnerAuthorizer : CustomAuthorizer<AppContext, IAmDish>, IsDishOwned
{
    private readonly IRepository<Dish> dishes;

    public DishOwnerAuthorizer(IRepository<Dish> dishes)
    {
        this.dishes = dishes;
    }

    protected override async Task<bool> CheckIfAuthorizedAsync(
        AppContext context,
        IAmDish obj)
    {
        var dish = await dishes.FindAsync(obj.DishId);
        return dish.OwnerId == context.UserId;
    }
}
```

Both queries and commands can (and should!) be behind authorization. By default, authorization is run before validation so the object that the command/query is pointing at might not exist.

### Context

The context allows CQRS-style API to be used by different parties. The handling part does not need to know whether it is invoked by some HTTP request or just in-proc. It is infrastructure's responsibility to provide and populate context based on the protocol.

This means that context is a good place to put some caller-related data (e.g. `UserId`) or inject some call-related data to the handler, based on the object payload or even HTTP headers.
