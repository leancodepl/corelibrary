# Operation

Operations change the state of the system, but also allow to return some result. If executing operation results in executing a command, it's the developer responsibility to decide how to handle potential validation errors.

## Potential use cases

1. Acting as a proxy for multiple commands (i.e. working on different domains) which are supposed to run at the same time and then returning the overall returning result, potentially making clients life's easier (one HTTP request, clean indication that these things are connected).
2. Running a command creating an object and immediately returning it. Note that in general our approach is command + immediate query with client generated id, but there might be reasons to violate this rule.
3. Integration with external services that do not conform to the CQRS pattern. E.g. creating payment in a third-party API and immediately returning some result which should not be stored in our system's database.
4. Integration with services which combine object validation and creation steps, making it impossible to validate command separately in command validator

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.Contracts | [![NuGet version (LeanCode.Contracts)](https://img.shields.io/nuget/vpre/LeanCode.Contracts.svg?style=flat-square)](https://www.nuget.org/packages/LeanCode.Contracts/2.0.0-preview.3/) | `IOperation` |
| LeanCode.CQRS.Execution | [![NuGet version (LeanCode.CQRS.Execution)](https://img.shields.io/nuget/vpre/LeanCode.CQRS.Execution.svg?style=flat-square)](https://www.nuget.org/packages/LeanCode.CQRS.Execution/8.0.2260-preview/) | `IOperationHandler` |

## Contract

Consider the operation that creates payment in external service for employee's access to application and returns payment token:

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

## Naming conventions

Operations are designed to both modify the state of the system and provide a result. To uphold a consistent naming convention, these operations should be named to reflect both their transformative action and if possible the nature of the information returned. Striking a balance between clarity and conciseness, names like `GenerateReferralLink`, `GetNextQuestion`, or `AnswerQuestion` convey both the intent of state modification and the potential for a consequential result. Correspondingly, handlers for operations should start with the name of the associated operation while incorporating the `OH` suffix.
