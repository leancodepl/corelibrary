# Operation

Operations change the state of the system, but also allow to return some result. If executing operation results in executing a command, it's the developer responsibility to decide how to handle potential validation errors.

## Potential use cases

1. Acting as a proxy for multiple commands (i.e. working on different domains) which are supposed to run at the same time and then returning the overall returning result, potentially making clients life's easier (one HTTP request, clean indication that these things are connected).
2. Running a command creating an object and immediately returning it. Note that in general our approach is command + immediate query with client generated id, but there might be reasons to violate this rule.
3. Integration with external services that do not conform to the CQRS pattern. E.g. creating payment in a third-party API and immediately returning some result which should not be stored in our system's database.
4. Integration with services which combine object validation and creation steps, making it impossible to validate command separately in command validator

## Contract

Consider the operation that creates payment in external service for employee's access to application and returns payment token.

```csharp
[AuthorizeWhenHasAnyOf(Auth.Roles.Admin)]
public class PayForAccess : IOperation<PaymentTokenDTO>
{
    public string EmployeeId { get; set; }
}

public class PaymentTokenDTO
{
    public string Token { get; set; }
}
```

## Handler

Operation handlers execute complex operations. They should not contain logic themselves, instead they should orchestrate commands, queries (and potentially other services) via `ICommandExecutor`, `IQueryExecutor` interfaces.

For the above operation, you can have handler like this:

```csharp
public class PayForAccessOH : IOperationHandler<PayForAccess, PaymentTokenDTO>
{
    private readonly IPaymentsService payments;

    public PayForAccessOH(IPaymentsService payments)
    {
        this.payments = payments;
    }

    public async Task<PaymentTokenDTO> ExecuteAsync(
        HttpContext context,
        PayForAccess operation)
    {
        var result = await payments.CreatePaymentInExternalService(
            operation.EmployeeId,
            context.RequestAborted);

        return new PaymentTokenDTO
        {
            Token = result.Token,
        };
    }
}
```
