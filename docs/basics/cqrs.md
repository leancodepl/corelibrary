# CQRS

We use CQRS in almost every project. We try to stick to it as much as possible because it allows us to easily construct our APIs and makes our code maintainable. Here, we present a short description on what CQRS is and how we designed it.

This is not an extensive CQRS description. :)

You can find more about how CQRS allows us to define the API in the [ContractsGenerator documentation](https://github.com/leancodepl/contractsgenerator/blob/main/docs/index.md).

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

### Operation

The strict separation of command and query has sometimes led to awkwardness for clients consuming the API. So, the type `IOperation` was introduced to add more flexibility. Operations change the state of the system, but also allow to return some result. Operations itself are not validated, if executing operation results in executing a command, it's the developer responsibility to decide how to handle potential validation errors.

```csharp
public class PayForOrder : IOperation<PaymentTokenDTO>
{
    public Guid OrderId { get; set; }
}

public class PaymentTokenDTO { }
```

Potential use cases:

1. Acting as a proxy for multiple commands (i.e. working on different domains) which are supposed to run at the same time and then returning the overall returning result, potentially making clients life's easier (one HTTP request, clean indication that these things are connected).
2. Running a command creating an object and immediately returning it. Note that in general our approach is command + immediate query with client generated id, but there might be reasons to violate this rule.
3. Integration with external services that do not conform to the CQRS pattern. E.g. creating payment in a third-party API and immediately returning some result which should not be stored in our system's database.
4. Integration with services which combine object validation and creation steps, making it impossible to validate command separately in command validator

## Implementation

Commands and queries define the API surface but they aren't the actual code that will be executed. That's the reason why there are command, query and operation handlers.

For each command/query/operation there must be exactly one handler. Handler is just a simple class that implements `ICommandHandler<TCommand>`, `IQueryHandler<TQuery, TResult>` or `IOperationHandler<TOperation, TResult>` interface. Its responsibility is to fulfill the premise that the command has, provide data for the query or execute the operation.

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

    public Task ExecuteAsync(HttpContext context, CreateDish command)
    {
        // context.GetUserId() is an extension method defined elsewhere
        var dish = Dish.Create(command.DishId, command.Name, context.GetUserId());

        // We only notify the repository that this is new entity, we let other part of the code commit the database transaction
        dishes.Add(dish);

        // `Execute` operations are async by nature, but here we don't need it
        return Task.CompletedTask;
    }
}
```

As you can see, the command handler is really simple - it just converts the command into new aggregate, tracking who owns the dish (`UserId` - they are the ones that have `CreateDish` permission). That does not mean this is the only responsibility of the handlers (it's just an example), but there are some guidelines related to them:

1. Keep them simple and testable, do not try to model whole flows with a single command,
2. Commands should rely on aggregates to gather the data (try not to use queries inside command handlers),
3. Commands should modify just a single aggregate (try to `Add`/`Update`/`Delete` at most once),
4. If the business process requires to modify multiple aggregates, try to use events and event handlers (but don't over-engineer),
5. If that does not help, modify/add/delete multiple aggregates,
6. Do not throw exceptions from inside commands. The client will receive generic error (`500 Internal Server Error`). Do it only as a last resort.

#### Validation

To reject commands that have invalid data or that cannot be fulfilled (the state of the system disallows it), you should use command validators. A command validator is instantiated and run before command handler even sees the command (but, by default, after authorization) and can return error code along the error message, so the system has an opportunity to inform the client why the command is invalid. A validator is a class that implements the `ICommandValidator<TCommand>` interface. To simplify things, we use `FluentValidation` so it is only necessary to implement `AbstractValidator<TCommand>`, everything else is handled by infrastructure.

To validate the command above, you can use something like

```csharp
public class CreateDishCV : AbstractValidator<CreateDish>
{
    private readonly IRepository<Dish> dishes;

    public CreateDishCV(IRepository<Dish> dishes)
    {
        this.dishes = dishes;

        RuleFor(c => c.Name)
            .NotNull().WithCode(CreateDish.ErrorCodes.InvalidName)
            .NotEmpty().WithCode(CreateDish.ErrorCodes.InvalidName);

        RuleFor(c => c.DishId)
            .CustomAsync(CheckDishDoesNotExistAsync);
    }

    private async Task CheckDishDoesNotExistAsync(
        Guid dishId,
        ValidationContext<CreateDish> ctx,
        CancellationToken ct
    )
    {
        var existing = await dishes.FindAsync(dishId, ct);
        if (existing is not null)
        {
            ctx.AddValidationError(
                $"A dish with the ID {dishId} already exists.",
                CreateDish.ErrorCodes.DishAlreadyExists
            );
        }
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

If you need complex validation logic that needs to access external state, use `MustAsync`/`CustomAsync` validators. For `CustomAsync` validators, you can use `AddValidationError` helper to specify the error code.

### Query handlers

Query handlers execute queries. They should not have any side effects but can return data back to the client. Since they can return data to the client, they don't need separate validation (handler can do it internally). In query handlers you don't need to operate on aggregate level (as this is read-side and is relatively DDD-free) and are allowed to perform arbitrary SQL queries.

Example query handler:

```csharp
public class DishesMatchingNameQH : IQueryHandler<DishesMatchingName, List<DishInfoDTO>>
{
    private readonly CoreDbContext dbContext;

    public FindDishesMatchingNameQH(CoreDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<List<DishInfoDTO>> ExecuteAsync(AppContext context, DishesMatchingName query)
    {
        // Here, we use raw SQL but you are free to use other mechanisms to get the data

        var filter = $"%{query.NameFilter.ToLower()}%";
        var results = await dbContext.QueryAsync<DishInfoDTO>(@"
            SELECT ""Id"" AS ""DishId"", ""Name"" FROM ""Dishes""
            WHERE ""Name"" LIKE @filter",
            new { filter });
        return results.AsList();
    }
}
```

### Operation handlers

Operation handlers execute complex operations. They should not contain logic themselves, instead they should orchestrate commands, queries (and potentially other services) via `ICommandExecutor`, `IQueryExecutor` interfaces.

Example operation handler:

```csharp
public class PayForOrderOH : IOperationHandler<PayForOrder, PaymentTokenDTO>
{
    private readonly IPaymentsService payments;

    public PayForOrderOH(IPaymentsService payments)
    {
        this.payments = payments;
    }

    public async Task<PaymentTokenDTO> ExecuteAsync(AppContext context, PayForOrder operation)
    {
        var result = await payments.CreatePaymentInExternalService(operation.OrderId);
        return new PaymentTokenDTO
        {
            // cut
        };
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
public interface IDishIsOwned
{
    // Object that use `DishIsOwned` attribute must implement this interface
    public interface IAmDish
    {
        Guid DishId { get; }
    }
}

[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
sealed class DishIsOwnedAttribute : AuthorizeWhenAttribute
{
    public DishIsOwned()
        : base(typeof(IDishIsOwned.IAmDish))
    { }
}
```

Sample usage:

```csharp
[DishIsOwned]
public class UpdateDishName : ICommand, IDishIsOwned.IAmDish
{
    public Guid DishId { get; set; }
    public string Name { get; set; }
}

public class DishIsOwnedAuthorizer : CustomAuthorizer<AppContext, DishIsOwned.IAmDish>, DishIsOwned
{
    private readonly IRepository<Dish> dishes;

    public DishIsOwnedAuthorizer(IRepository<Dish> dishes)
    {
        this.dishes = dishes;
    }

    protected override async Task<bool> CheckIfAuthorizedAsync(
        HttpContext context,
        DishIsOwned.IAmDish obj)
    {
        var dish = await dishes.FindAsync(obj.DishId);
        return dish.OwnerId == context.GetUserId();
    }
}
```

Both queries, commands and operations can (and should!) be behind authorization. By default, authorization is run before validation so the object that the command/query/operation is pointing at might not exist.

### Context

Before CoreLib v8, there existed a separate context type that allowed you to map data from the transport (i.e. HTTP). In v8, we ditched it in favor of an ordinary `HttpContext` - since the inception of CoreLib there were no real need of supporting different types of transport.

## Remote CQRS

Previously we used MVC approach, where controllers layer was responsible for authorization and invoked corresponding queries/commands. Controllers quickly have began to have only `RunAsync`/`GetAsync` invocations. This led us to abandon controllers and switch to just two endpoints that parsed body as command/query and executed it. Finally, we decided to ditch ASP.NET Core MVC in favor of plain ASP.NET Core and custom middleware that does exactly that - parse body, convert to correct command/query and execute it. We call this **RemoteCQRS**. It is a simple, JSON-based protocol for invoking commands and queries. It handles POST requests to `/command` or `/query` or `/operation` API endpoints, parses the object name from the URL (full namespace + name), decodes the body (only JSON encoding is supported) and executes `RunAsync`/`GetAsync` for given object.

There exist three marker interfaces:

```cs
public interface ICommand { }
public interface IQuery<out TResult> { }
public interface IOperation<out TResult> { }
```

All commands, queries and operations must derive from those interfaces and so they are available via RemoteCQRS.

RemoteCQRS request example:

```bash
curl -X POST \
  https://api.local.lncd.pl/api/query/Full.Object.Namespace.Name.FindDishesMatchingName \
  -H 'Content-Type: application/json' \
  -d '{ "NameFilter": "sushi" }'
```

The following request will execute the previously defined query and return the results. The API may return one of the
following HTTP Status Codes:

- 200 OK - command/query succeeded
- 400 Bad Request - request content is malformed
- 401 Unauthorized - client is not authorized
- 403 Forbidden - command/query authorization failed
- 405 Method Not Allowed - request Verb is not POST
- 422 Unprocessable Entity - command validation failed

You can find more about the contracts in the [ContractsGenerator documentation](https://github.com/leancodepl/contractsgenerator/blob/main/docs/index.md).
